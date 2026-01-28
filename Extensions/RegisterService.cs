using System.Security.Cryptography;
using System.Text;

namespace TokenManagerApi.Extensions;

public static class AuthenticationExtension
{
    public static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string hash)
        => HashPassword(password) == hash;
}
