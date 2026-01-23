using System.Security.Cryptography;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Aggregates.User;
public class User : AggregateRoot
{
    public string Email { get; private set; } = null!;
    public byte[] PasswordHash { get; private set; } = new byte[32];
    public byte[] PasswordSalt { get; private set; } = new byte[32];
    public string VerificationToken { get; private set; } = CreateRandomToken();
    public DateTime? VerifiedAt { get; private set; }
    public string? ResetPasswordToken { get; }

    public UserMetadata? Metadata { get; private set; }
    public ICollection<UserRole> Roles { get; private set; } = [];

    private User(string email, byte[] passwordHash, byte[] passwordSalt)
    {
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    private static string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

    public static Result<User> Create(
        string email,
        byte[] passwordHash,
        byte[] passwordSalt,
        string? name,
        DateOnly birthDate,
        Guid roleId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(UserErrors.EmptyUserEmail);
        }

        if (passwordHash is null || passwordHash.Length == 0 || 
            passwordSalt is null || passwordSalt.Length == 0)
        {
            return Result.Failure<User>(UserErrors.EmptyPassword);
        }

        var user = new User(email, passwordHash, passwordSalt);

        var userSetMetadataResult = user.SetMetadata(name, birthDate);
        if (userSetMetadataResult.IsFailure)
        {
            return Result.Failure<User>(userSetMetadataResult.Error);
        }

        var addRoleResult = user.AddRole(roleId);
        if (addRoleResult.IsFailure)
        {
            return Result.Failure<User>(addRoleResult.Error);
        }

        user.RaiseDomainEvent(new SendUserEmailVerificationDomainEvent(
            user.Id,
            user.Email,
            user.VerificationToken,
            user.Metadata!.Name ?? string.Empty));

        return Result.Success(user);
    }

    private Result SetMetadata(string? name, DateOnly birthDate)
    {
        var userMetadataResult = UserMetadata.Create(Id, name, birthDate);
        if (userMetadataResult.IsFailure)
        {
            return Result.Failure<UserMetadata>(userMetadataResult.Error);
        }

        Metadata = userMetadataResult.Value;
        return Result.Success();
    }

    private Result AddRole(Guid roleId)
    {
        if (Roles.Any(userRole => userRole.RoleId == roleId))
        {
            return Result.Failure<UserMetadata>(UserErrors.UserRoleAlreadyExists);
        }

        var userRoleResult = UserRole.Create(Id, roleId);
        if (userRoleResult.IsFailure)
        {
            return Result.Failure<UserMetadata>(userRoleResult.Error);
        }

        Roles.Add(userRoleResult.Value);
        return Result.Success();
    }

    public Result Verify()
    {
        if (VerifiedAt is not null)
        {
            return Result.Failure(UserErrors.UserAlreadyVerified);
        }

        var utcTime = DateTime.UtcNow;

        VerifiedAt = utcTime;
        UpdatedAt = utcTime;

        RaiseDomainEvent(new CreateUserProfileDomainEvent(Id, Metadata?.Name ?? string.Empty));

        return Result.Success();
    }
}
