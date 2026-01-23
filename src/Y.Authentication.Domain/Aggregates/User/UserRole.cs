using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Aggregates.User;
public class UserRole : Entity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public User User { get; set; } = null!;
    public Aggregates.Role.Role Role { get; set; } = null!;

    private UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    internal static Result<UserRole> Create(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<UserRole>(UserErrors.EmptyUser);
        }

        if (roleId == Guid.Empty)
        {
            return Result.Failure<UserRole>(UserErrors.EmptyUserRole);
        }

        return Result.Success(new UserRole(userId, roleId));
    }
}
