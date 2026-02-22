using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Aggregates.User;
public class UserMetadata : Entity
{
    public const int UserNameMaxLength = 50;

    public string Name { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private UserMetadata(Guid userId, string? name, DateOnly birthDate)
    {
        UserId = userId;
        Name = name ?? string.Empty;
        BirthDate = birthDate;
    }

    public static Result<UserMetadata> Create(Guid userId, string? name, DateOnly birthDate)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<UserMetadata>(UserErrors.UserMetadataEmptyUser);
        }

        if (name?.Length > UserNameMaxLength)
        {
            return Result.Failure<UserMetadata>(UserErrors.UserNameLengthExceeded);
        }

        return Result.Success(new UserMetadata(userId, name, birthDate));
    }
}
