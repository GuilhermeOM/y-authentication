using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Aggregates.Role;
public class Role : AggregateRoot
{
    public string Name { get; private set; }

    private Role(string name)
    {
        Name = name;
    }

    public static Result<Role> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Role>(RoleErrors.EmptyRoleName);
        }

        return Result.Success(new Role(name));
    }
}
