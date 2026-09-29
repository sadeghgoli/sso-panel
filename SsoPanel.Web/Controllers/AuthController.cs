using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data;
using SsoPanel.Web.Data.Entities;
using SsoPanel.Web.Models;

namespace SsoPanel.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly PanelDbContext _db;
    private readonly ILogger<AuthController> _logger;
    private readonly PasswordHasher<AdminUser> _hasher = new();

    public AuthController(PanelDbContext db, ILogger<AuthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var username = model.Username.Trim();
        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username);
        var result = user == null
            ? PasswordVerificationResult.Failed
            : _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

        if (user == null || result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Failed panel login for {Username} from {Ip}", username, HttpContext.Connection.RemoteIpAddress);
            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است");
            return View(model);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            await _db.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, "Admin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("change-password")]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.AdminUsers.FindAsync(id);
        if (user == null)
            return RedirectToAction(nameof(Login));

        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "رمز فعلی اشتباه است");
            return View(model);
        }

        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        await _db.SaveChangesAsync();
        TempData["Success"] = "رمز عبور با موفقیت تغییر کرد";
        return RedirectToAction("Index", "Dashboard");
    }
}
