using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data;
using SsoPanel.Web.Models;

namespace SsoPanel.Web.Controllers;

public class LogsController : Controller
{
    private const int PageSize = 50;

    private readonly PanelDbContext _db;

    public LogsController(PanelDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int? clientId, bool onlyRejected = false, int page = 1)
    {
        page = Math.Max(1, page);

        var query = _db.ApiRequestLogs.AsNoTracking().AsQueryable();
        if (clientId.HasValue)
            query = query.Where(l => l.ClientId == clientId);
        if (onlyRejected)
            query = query.Where(l => l.StatusCode == 401 || l.StatusCode == 403);

        var model = new LogsViewModel
        {
            ClientId = clientId,
            OnlyRejected = onlyRejected,
            Page = page,
            PageSize = PageSize,
            TotalCount = await query.CountAsync(),
            Items = await query
                .Include(l => l.Client)
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(),
            Clients = await _db.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync()
        };

        return View(model);
    }
}
