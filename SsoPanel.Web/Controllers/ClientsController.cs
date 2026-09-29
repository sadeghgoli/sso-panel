using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data;
using SsoPanel.Web.Data.Entities;
using SsoPanel.Web.Models;

namespace SsoPanel.Web.Controllers;

public class ClientsController : Controller
{
    public const string TempDataNewKey = "NewPlainKey";

    private readonly PanelDbContext _db;

    public ClientsController(PanelDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var clients = await _db.Clients
            .AsNoTracking()
            .Include(c => c.ApiKeys)
            .Include(c => c.Domains)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(clients);
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new ClientFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(ClientFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Form", model);

        var client = new Client();
        Apply(model, client);
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        TempData["Success"] = "سرویس‌گیرنده ایجاد شد. اکنون می‌توانید کلید API یا دامنه مجاز تعریف کنید.";
        return RedirectToAction(nameof(Details), new { id = client.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return NotFound();

        return View("Form", new ClientFormViewModel
        {
            Id = client.Id,
            Name = client.Name,
            ContactName = client.ContactName,
            ContactPhone = client.ContactPhone,
            Description = client.Description,
            IsActive = client.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ClientFormViewModel model)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View("Form", model);
        }

        Apply(model, client);
        await _db.SaveChangesAsync();
        TempData["Success"] = "اطلاعات سرویس‌گیرنده ذخیره شد";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (client == null)
            return NotFound();

        var model = new ClientDetailsViewModel
        {
            Client = client,
            ApiKeys = await _db.ApiKeys.AsNoTracking()
                .Where(k => k.ClientId == id)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync(),
            Domains = await _db.AllowedDomains.AsNoTracking()
                .Where(d => d.ClientId == id)
                .OrderBy(d => d.Origin)
                .ToListAsync(),
            RecentRequests = await _db.ApiRequestLogs.AsNoTracking()
                .Where(l => l.ClientId == id)
                .OrderByDescending(l => l.CreatedAt)
                .Take(20)
                .ToListAsync(),
            NewPlainKey = TempData[TempDataNewKey] as string,
            NewKey = new CreateApiKeyViewModel { ClientId = id },
            NewDomain = new AddDomainViewModel { ClientId = id }
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return NotFound();

        client.IsActive = !client.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = client.IsActive
            ? "سرویس‌گیرنده فعال شد"
            : "سرویس‌گیرنده غیرفعال شد؛ کلیدها و دامنه‌های آن دیگر پذیرفته نمی‌شوند";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return NotFound();

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"سرویس‌گیرنده «{client.Name}» حذف شد";
        return RedirectToAction(nameof(Index));
    }

    private static void Apply(ClientFormViewModel model, Client client)
    {
        client.Name = model.Name.Trim();
        client.ContactName = model.ContactName?.Trim();
        client.ContactPhone = model.ContactPhone?.Trim();
        client.Description = model.Description?.Trim();
        client.IsActive = model.IsActive;
    }
}
