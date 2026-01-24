using System.Net;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class UserErrors
{
    public static Error EmptyUserEmail => new("EMPTY_USER_EMAIL", "User email can not be empty");
    public static Error EmptyUserRole => new("EMPTY_USER_ROLE", "User role can not be empty");
    public static Error EmptyUser => new("EMPTY_USER", "User can not be empty");
    public static Error EmptyPassword => new("EMPTY_PASSWORD", "Password can not be empty");
    public static Error UserMetadataWithoutUser => new("USER_METADATA_WITHOT_USER", "User metadata can not be created without user");

    public static Error UserNameLengthExceeded => new(HttpStatusCode.BadRequest, "USER_NAME_LENGTH_EXCEEDED", $"User name must be lower or equal {UserMetadata.UserNameMaxLength} characters");
    public static Error UserRoleNotFound => new(HttpStatusCode.InternalServerError, "USER_ROLE_NOT_FOUND", "User role not found");
    public static Error UserRoleAlreadyExists => new(HttpStatusCode.Conflict, "USER_ROLE_ALREADY_EXISTS", "User role already exists");
    public static Error UserAlreadyExists => new(HttpStatusCode.Conflict, "USER_ALREADY_EXISTS", "User already exists");
    public static Error UserNotFound => new(HttpStatusCode.NotFound, "USER_NOT_FOUND", "User not found");
    public static Error UserAlreadyVerified => new(HttpStatusCode.Conflict, "USER_ALREADY_VERIFIED", "User already verified");
    public static Error UserNotVerified => new(HttpStatusCode.Unauthorized, "USER_NOT_VERIFIED", "User is not verified");
    public static Error UserPasswordNotValid => new(HttpStatusCode.Unauthorized, "USER_PASSWORD_NOT_VALID", "User password is not valid");
}
