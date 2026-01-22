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

    private UserMetadata(string name, DateOnly birthDate)
    {
        Name = name;
        BirthDate = birthDate;
    }

    public static Result<UserMetadata> Create(string? name, DateOnly birthDate)
    {
        name ??= string.Empty;

        if (name.Length > UserNameMaxLength)
        {
            return Result.Failure<UserMetadata>(UserErrors.UserNameLengthExceeded);
        }

        return Result.Success(new UserMetadata(name, birthDate));
    }
}
