using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data;
using SsoPanel.Web.Data.Entities;

namespace SsoPanel.Web.Services;

public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PanelDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync();

        if (await db.AdminUsers.AnyAsync())
            return;

        var username = config["SeedAdmin:Username"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No admin user exists and SeedAdmin is not configured; panel login is impossible until one is seeded.");
            return;
        }

        var admin = new AdminUser { Username = username.Trim() };
        admin.PasswordHash = new PasswordHasher<AdminUser>().HashPassword(admin, password);
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded admin user {Username}", admin.Username);
    }
}
