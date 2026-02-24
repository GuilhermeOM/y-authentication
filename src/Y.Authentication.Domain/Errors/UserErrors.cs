using System.Net;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class UserErrors
{
    public static Error UserAvatarNullUpload => new("USERAVATAR_NULL_UPLOAD", "User avatar upload can not be null");
    public static Error UserAvatarEmptyUrl => new("USERAVATAR_EMPTY_UPLOAD_URL", "User avatar must have an url");
    public static Error UserAvatarInvalidMimeType => new("USERAVATAR_INVALID_MIME_TYPE", "User avatar must have a valid mime type");
    public static Error UserMetadataEmptyUser => new("USERMETADATA_EMPTY_USER", "User metadata must have an user");
    public static Error UserRoleEmptyRole => new("USERROLE_EMPTY_ROLE", "User role must have a role");
    public static Error UserRoleEmptyUser => new("USERROLE_EMPTY_USER", "User role must have an user");
    public static Error UserEmptyEmail => new("USER_EMPTY_EMAIL", "User must have an email");
    public static Error UserEmptyPassword => new("USER_EMPTY_PASSWORD", "User must have a password");

    public static Error UserNameLengthExceeded => new(HttpStatusCode.BadRequest, "USER_NAME_LENGTH_EXCEEDED", $"User name must be lower or equal {UserMetadata.UserNameMaxLength} characters");
    public static Error UserRoleNotFound => new(HttpStatusCode.InternalServerError, "USER_ROLE_NOT_FOUND", "User role not found");
    public static Error UserRoleAlreadyExists => new(HttpStatusCode.Conflict, "USER_ROLE_ALREADY_EXISTS", "User role already exists");
    public static Error UserAlreadyExists => new(HttpStatusCode.Conflict, "USER_ALREADY_EXISTS", "User already exists");
    public static Error UserNotFound => new(HttpStatusCode.NotFound, "USER_NOT_FOUND", "User not found");
    public static Error UserAlreadyVerified => new(HttpStatusCode.Conflict, "USER_ALREADY_VERIFIED", "User already verified");
    public static Error UserNotVerified => new(HttpStatusCode.Unauthorized, "USER_NOT_VERIFIED", "User is not verified");
    public static Error UserPasswordNotValid => new(HttpStatusCode.Unauthorized, "USER_PASSWORD_NOT_VALID", "User password is not valid");
    public static Error UserCreationFailed => new(HttpStatusCode.InternalServerError, "USER_CREATION_FAILED", "An error occurred while creating the user");
    public static Error UserAvatarUnsupportedMimeType => new(HttpStatusCode.BadRequest, "USER_AVATAR_UNSUPPORTED_MIME_TYPE", "User avatar mime not supported");
}
