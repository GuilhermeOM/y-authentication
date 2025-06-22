using System.Net;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Errors;
public static class UserMetadataErrors
{
    public static Error UserMetadataCreationFailed => new(HttpStatusCode.InternalServerError, "USER_METADATA_CREATION_FAILED", "User metadata creation failed");
}
