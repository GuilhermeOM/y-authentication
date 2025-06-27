using System.Security.Cryptography;
using Y.Authentication.Domain.Entities.Base;

namespace Y.Authentication.Domain.Entities;
public class User : Entity
{
    public required string Email { get; set; }
    public byte[] PasswordHash { get; init; } = new byte[32];
    public byte[] PasswordSalt { get; init; } = new byte[32];
    public string VerificationToken { get; } = CreateRandomToken();
    public DateTime? VerifiedAt { get; set; }
    public string? ResetPasswordToken { get; }

    public UserMetadata? Metadata { get; set; }

    private static string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
}
