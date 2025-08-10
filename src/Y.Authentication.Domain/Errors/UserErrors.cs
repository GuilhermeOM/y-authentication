using System.Net;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class UserErrors
{
    public static Error UserAlreadyExists => new(HttpStatusCode.Conflict, "USER_ALREADY_EXISTS", "User already exists");
    public static Error UserCreationFailed => new(HttpStatusCode.InternalServerError, "USER_CREATION_FAILED", "User creation failed");
    public static Error UserNotFound => new(HttpStatusCode.NotFound, "USER_NOT_FOUND", "User not found");
    public static Error UserVerificationFailed => new(HttpStatusCode.InternalServerError, "USER_VERIFICATION_FAILED", "User verification failed");
    public static Error UserAlreadyVerified => new(HttpStatusCode.Conflict, "USER_ALREADY_VERIFIED", "User already verified");
    public static Error UserNotVerified => new(HttpStatusCode.Unauthorized, "USER_NOT_VERIFIED", "User is not verified");
    public static Error UserPasswordNotValid => new(HttpStatusCode.Unauthorized, "USER_PASSWORD_NOT_VALID", "User password is not valid");
}
