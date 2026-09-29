using Microsoft.AspNetCore.Mvc;
using SsoPanel.Web.Data;
using SsoPanel.Web.Data.Entities;
using SsoPanel.Web.Models;
using SsoPanel.Web.Services;

namespace SsoPanel.Web.Controllers;

public class ApiKeysController : Controller
{
    private readonly PanelDbContext _db;

    public ApiKeysController(PanelDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateApiKeyViewModel model)
    {
        var client = await _db.Clients.FindAsync(model.ClientId);
        if (client == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "عنوان کلید الزامی است";
            return RedirectToDetails(model.ClientId);
        }

        DateTime? expiresAt = null;
        if (model.ExpiresAt.HasValue)
        {
            // The picked date is inclusive: the key stays valid until the end of that local day.
            expiresAt = DateTime.SpecifyKind(model.ExpiresAt.Value.Date.AddDays(1), DateTimeKind.Local).ToUniversalTime();
            if (expiresAt <= DateTime.UtcNow)
            {
                TempData["Error"] = "تاریخ انقضا باید در آینده باشد";
                return RedirectToDetails(model.ClientId);
            }
        }

        var (plain, prefix, hash) = ApiKeyGenerator.Generate();
        _db.ApiKeys.Add(new ApiKey
        {
            ClientId = client.Id,
            Name = model.Name.Trim(),
            Prefix = prefix,
            KeyHash = hash,
            ExpiresAt = expiresAt
        });
        await _db.SaveChangesAsync();

        TempData[ClientsController.TempDataNewKey] = plain;
        return RedirectToDetails(client.Id);
    }

    [HttpPost]
    public async Task<IActionResult> Revoke(int id)
    {
        var key = await _db.ApiKeys.FindAsync(id);
        if (key == null)
            return NotFound();

        key.RevokedAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"کلید «{key.Name}» باطل شد (اعمال در سرویس SSO حداکثر ظرف یک دقیقه)";
        return RedirectToDetails(key.ClientId);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var key = await _db.ApiKeys.FindAsync(id);
        if (key == null)
            return NotFound();

        if (key.RevokedAt == null)
        {
            TempData["Error"] = "ابتدا کلید را باطل کنید";
            return RedirectToDetails(key.ClientId);
        }

        _db.ApiKeys.Remove(key);
        await _db.SaveChangesAsync();
        TempData["Success"] = "کلید حذف شد";
        return RedirectToDetails(key.ClientId);
    }

    private IActionResult RedirectToDetails(int clientId) =>
        RedirectToAction("Details", "Clients", new { id = clientId });
}
