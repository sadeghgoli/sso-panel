using System.ComponentModel.DataAnnotations;
using SsoPanel.Web.Data.Entities;

namespace SsoPanel.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [Display(Name = "نام کاربری")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "رمز فعلی الزامی است")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز جدید الزامی است")]
    [MinLength(8, ErrorMessage = "رمز جدید حداقل ۸ کاراکتر باشد")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "تکرار رمز با رمز جدید یکسان نیست")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ClientFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "نام سرویس‌گیرنده الزامی است")]
    [MaxLength(200)]
    [Display(Name = "نام سرویس‌گیرنده")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "نام رابط")]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    [Display(Name = "تلفن رابط")]
    public string? ContactPhone { get; set; }

    [MaxLength(2000)]
    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "فعال")]
    public bool IsActive { get; set; } = true;
}

public class CreateApiKeyViewModel
{
    public int ClientId { get; set; }

    [Required(ErrorMessage = "عنوان کلید الزامی است")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? ExpiresAt { get; set; }
}

public class AddDomainViewModel
{
    public int ClientId { get; set; }

    [Required(ErrorMessage = "دامنه الزامی است")]
    [MaxLength(300)]
    public string Origin { get; set; } = string.Empty;

    public bool AllowSubdomains { get; set; }
}

public class ClientDetailsViewModel
{
    public Client Client { get; set; } = null!;
    public List<ApiKey> ApiKeys { get; set; } = new();
    public List<AllowedDomain> Domains { get; set; } = new();
    public List<ApiRequestLog> RecentRequests { get; set; } = new();
    public string? NewPlainKey { get; set; }
    public CreateApiKeyViewModel NewKey { get; set; } = new();
    public AddDomainViewModel NewDomain { get; set; } = new();
}

public class DashboardViewModel
{
    public int ClientCount { get; set; }
    public int ActiveClientCount { get; set; }
    public int ActiveKeyCount { get; set; }
    public int ActiveDomainCount { get; set; }
    public int RequestsLast24h { get; set; }
    public int RejectedLast24h { get; set; }
    public List<ApiRequestLog> RecentRequests { get; set; } = new();
}

public class LogsViewModel
{
    public List<ApiRequestLog> Items { get; set; } = new();
    public List<Client> Clients { get; set; } = new();
    public int? ClientId { get; set; }
    public bool OnlyRejected { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public class DocsViewModel
{
    public string SsoBaseUrl { get; set; } = string.Empty;
}
