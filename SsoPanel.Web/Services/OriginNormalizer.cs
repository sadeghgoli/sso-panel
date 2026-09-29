namespace SsoPanel.Web.Services;

public static class OriginNormalizer
{
    /// <summary>
    /// Converts user input (e.g. "app.example.ir", "https://app.example.ir/path") to "scheme://host[:port]".
    /// Only https is accepted, except for localhost/127.0.0.1 which may use http.
    /// </summary>
    public static bool TryNormalize(string? input, out string origin, out string? error)
    {
        origin = string.Empty;
        error = null;

        var value = input?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            error = "دامنه الزامی است";
            return false;
        }

        if (!value.Contains("://", StringComparison.Ordinal))
            value = "https://" + value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || string.IsNullOrEmpty(uri.Host))
        {
            error = "فرمت دامنه نامعتبر است";
            return false;
        }

        var isLocal = uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        if (uri.Scheme != Uri.UriSchemeHttps && !(isLocal && uri.Scheme == Uri.UriSchemeHttp))
        {
            error = "فقط دامنه‌های https مجاز هستند (به جز localhost)";
            return false;
        }

        if (uri.Host.StartsWith("*.", StringComparison.Ordinal) || uri.Host.Contains('*'))
        {
            error = "برای زیردامنه‌ها از گزینه «شامل زیردامنه‌ها» استفاده کنید";
            return false;
        }

        origin = uri.IsDefaultPort
            ? $"{uri.Scheme}://{uri.Host.ToLowerInvariant()}"
            : $"{uri.Scheme}://{uri.Host.ToLowerInvariant()}:{uri.Port}";
        return true;
    }
}
