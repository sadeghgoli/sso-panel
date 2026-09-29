using Microsoft.EntityFrameworkCore;
using SsoPanel.Web.Data.Entities;

namespace SsoPanel.Web.Data;

/// <summary>
/// Schema is also read by mvc-web-sso (ClientAccessService) with raw SQL,
/// so table/column names (snake_case) must stay in sync with its queries.
/// </summary>
public class PanelDbContext : DbContext
{
    public PanelDbContext(DbContextOptions<PanelDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<AllowedDomain> AllowedDomains => Set<AllowedDomain>();
    public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AdminUser>(e =>
        {
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });

        b.Entity<Client>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ContactName).HasMaxLength(200);
            e.Property(x => x.ContactPhone).HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(2000);
        });

        b.Entity<ApiKey>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Prefix).HasMaxLength(16).IsRequired();
            e.Property(x => x.KeyHash).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.KeyHash).IsUnique();
            e.HasOne(x => x.Client).WithMany(c => c.ApiKeys).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
            e.Ignore(x => x.IsUsable);
        });

        b.Entity<AllowedDomain>(e =>
        {
            e.Property(x => x.Origin).HasMaxLength(300).IsRequired();
            e.HasIndex(x => x.Origin).IsUnique();
            e.HasOne(x => x.Client).WithMany(c => c.Domains).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ApiRequestLog>(e =>
        {
            e.Property(x => x.Origin).HasMaxLength(300);
            e.Property(x => x.Path).HasMaxLength(500).IsRequired();
            e.Property(x => x.Ip).HasMaxLength(64);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.ClientId);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
