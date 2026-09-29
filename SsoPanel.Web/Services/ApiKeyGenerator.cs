using System.Security.Cryptography;
using System.Text;

namespace SsoPanel.Web.Services;

public static class ApiKeyGenerator
{
    public const string KeyPrefix = "sso_";

    public static (string PlainKey, string Prefix, string Hash) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var body = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var plain = KeyPrefix + body;
        return (plain, plain[..12], Hash(plain));
    }

    /// <summary>Must match the hashing in mvc-web-sso ClientAccessService.</summary>
    public static string Hash(string plainKey) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plainKey))).ToLowerInvariant();
}
