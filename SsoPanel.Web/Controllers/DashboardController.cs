using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data;
using SsoPanel.Web.Models;

namespace SsoPanel.Web.Controllers;

public class DashboardController : Controller
{
    private readonly PanelDbContext _db;

    public DashboardController(PanelDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var since = now.AddHours(-24);

        var model = new DashboardViewModel
        {
            ClientCount = await _db.Clients.CountAsync(),
            ActiveClientCount = await _db.Clients.CountAsync(c => c.IsActive),
            ActiveKeyCount = await _db.ApiKeys.CountAsync(k =>
                k.RevokedAt == null && (k.ExpiresAt == null || k.ExpiresAt > now) && k.Client!.IsActive),
            ActiveDomainCount = await _db.AllowedDomains.CountAsync(d => d.IsActive && d.Client!.IsActive),
            RequestsLast24h = await _db.ApiRequestLogs.CountAsync(l => l.CreatedAt >= since),
            RejectedLast24h = await _db.ApiRequestLogs.CountAsync(l => l.CreatedAt >= since && (l.StatusCode == 401 || l.StatusCode == 403)),
            RecentRequests = await _db.ApiRequestLogs.AsNoTracking()
                .Include(l => l.Client)
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .ToListAsync()
        };

        return View(model);
    }
}
