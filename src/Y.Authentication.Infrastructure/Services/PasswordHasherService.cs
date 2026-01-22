using System.Security.Cryptography;
using System.Text;
using Y.Authentication.Domain.Services;

namespace Y.Authentication.Infrastructure.Services;
internal sealed class PasswordHasherService : IPasswordHasherService
{
    public (byte[] Salt, byte[] Hash) HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (salt, hash);
    }

    public bool IsPasswordSequenceEqual(string password, byte[] salt, byte[] hash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(hash);
    }
}
