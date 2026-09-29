using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data;
using SsoPanel.Web.Data.Entities;
using SsoPanel.Web.Models;
using SsoPanel.Web.Services;

namespace SsoPanel.Web.Controllers;

public class DomainsController : Controller
{
    private readonly PanelDbContext _db;

    public DomainsController(PanelDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddDomainViewModel model)
    {
        var client = await _db.Clients.FindAsync(model.ClientId);
        if (client == null)
            return NotFound();

        if (!OriginNormalizer.TryNormalize(model.Origin, out var origin, out var error))
        {
            TempData["Error"] = error;
            return RedirectToDetails(client.Id);
        }

        var existing = await _db.AllowedDomains
            .Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Origin == origin);
        if (existing != null)
        {
            TempData["Error"] = existing.ClientId == client.Id
                ? $"دامنه {origin} قبلا ثبت شده است"
                : $"دامنه {origin} قبلا برای سرویس‌گیرنده «{existing.Client?.Name}» ثبت شده است";
            return RedirectToDetails(client.Id);
        }

        _db.AllowedDomains.Add(new AllowedDomain
        {
            ClientId = client.Id,
            Origin = origin,
            AllowSubdomains = model.AllowSubdomains
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"دامنه {origin} اضافه شد (اعمال در سرویس SSO حداکثر ظرف یک دقیقه)";
        return RedirectToDetails(client.Id);
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var domain = await _db.AllowedDomains.FindAsync(id);
        if (domain == null)
            return NotFound();

        domain.IsActive = !domain.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = domain.IsActive ? $"دامنه {domain.Origin} فعال شد" : $"دامنه {domain.Origin} غیرفعال شد";
        return RedirectToDetails(domain.ClientId);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var domain = await _db.AllowedDomains.FindAsync(id);
        if (domain == null)
            return NotFound();

        _db.AllowedDomains.Remove(domain);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"دامنه {domain.Origin} حذف شد";
        return RedirectToDetails(domain.ClientId);
    }

    private IActionResult RedirectToDetails(int clientId) =>
        RedirectToAction("Details", "Clients", new { id = clientId });
}
