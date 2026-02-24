using System.Security.Cryptography;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

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
    public UserAvatar? Avatar { get; private set; }
    public ICollection<UserRole> Roles { get; private set; } = [];

    private User(string email, byte[] passwordHash, byte[] passwordSalt)
    {
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    private static string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

    public static Result<User> Create(
        PasswordHash passwordHash,
        string email,
        DateOnly birthDate,
        Guid roleId,
        string? name = null,
        FileUpload? avatarUpload = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(UserErrors.UserEmptyEmail);
        }

        if (passwordHash.Hash is null || passwordHash.Hash.Length == 0 ||
            passwordHash.Salt is null || passwordHash.Salt.Length == 0)
        {
            return Result.Failure<User>(UserErrors.UserEmptyPassword);
        }

        var user = new User(email, passwordHash.Hash, passwordHash.Salt);

        var userSetMetadataResult = user.SetMetadata(name, birthDate);
        if (userSetMetadataResult.IsFailure)
        {
            return Result.Failure<User>(userSetMetadataResult.Error);
        }

        var userSetAvatarResult = user.SetAvatar(avatarUpload);
        if (userSetAvatarResult.IsFailure)
        {
            return Result.Failure<User>(userSetAvatarResult.Error);
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

    private Result SetAvatar(FileUpload? avatarUpload)
    {
        if (avatarUpload is null)
        {
            return Result.Success();
        }

        var userAvatarResult = UserAvatar.Create(Id, avatarUpload);
        if (userAvatarResult.IsFailure)
        {
            return Result.Failure<UserAvatar>(userAvatarResult.Error);
        }

        Avatar = userAvatarResult.Value;
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

        return Result.Success();
    }
}
