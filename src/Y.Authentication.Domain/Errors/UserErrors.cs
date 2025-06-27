using System.Net;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class UserErrors
{
    public static Error UserAlreadyExists => new(HttpStatusCode.Conflict, "USER_ALREADY_EXISTS", "User already exists");
    public static Error UserCreationFailed => new(HttpStatusCode.InternalServerError, "USER_CREATION_FAILED", "User creation failed");
}
