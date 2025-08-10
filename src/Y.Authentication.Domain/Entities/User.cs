using System.Security.Cryptography;
using System.Text;
using Y.Authentication.Domain.Entities.Base;

namespace Y.Authentication.Domain.Entities;
public class User : Entity
{
    public required string Email { get; set; }
    public byte[] PasswordHash { get; init; } = new byte[32];
    public byte[] PasswordSalt { get; init; } = new byte[32];
    public string VerificationToken { get; private set; } = CreateRandomToken();
    public DateTime? VerifiedAt { get; set; }
    public string? ResetPasswordToken { get; }

    public UserMetadata? Metadata { get; set; }
    public ICollection<UserRole> Roles { get; set; } = [];

    private static string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

    public bool IsPasswordValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        using var hmac = new HMACSHA512(PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(PasswordHash);
    }
}
