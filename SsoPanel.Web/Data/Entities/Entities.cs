namespace SsoPanel.Web.Data.Entities;

public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ApiKey> ApiKeys { get; set; } = new();
    public List<AllowedDomain> Domains { get; set; } = new();
}

public class ApiKey
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsUsable => RevokedAt == null && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);
}

public class AllowedDomain
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public string Origin { get; set; } = string.Empty;
    public bool AllowSubdomains { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApiRequestLog
{
    public long Id { get; set; }
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public int? ApiKeyId { get; set; }
    public string? Origin { get; set; }
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? Ip { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
