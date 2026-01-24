using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class RoleErrors
{
    public static Error EmptyRoleName => new("EMPTY_ROLE_NAME", "Role name can not be empty");
}
