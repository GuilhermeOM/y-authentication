using System.Net;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class RoleErrors
{
    public static Error EmptyRoleName => new("EMPTY_ROLE_NAME", "Role name can not be empty");

    public static Error RoleAlreadyExists => new(HttpStatusCode.Conflict, "ROLE_ALREADY_EXISTS", "Role already exists");
    public static Error RoleCreationFailed => new(HttpStatusCode.InternalServerError, "ROLE_CREATION_FAILED", "Role creation failed");
    public static Error RoleNotFound => new(HttpStatusCode.NotFound, "ROLE_NOT_FOUND", "Role not found");
}
