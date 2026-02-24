using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Domain.Aggregates.User;
public class UserAvatar : Entity
{
    private static readonly HashSet<string> _supportedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public string Mime { get; private set; } = string .Empty;
    public string Url { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private UserAvatar()
    {
    }

    public static Result<UserAvatar> Create(Guid userId, FileUpload mediaUpload)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<UserAvatar>(UserErrors.UserMetadataEmptyUser);
        }

        if (mediaUpload is null)
        {
            return Result.Failure<UserAvatar>(UserErrors.UserAvatarNullUpload);
        }

        if (string.IsNullOrWhiteSpace(mediaUpload.Url))
        {
            return Result.Failure<UserAvatar>(UserErrors.UserAvatarEmptyUrl);
        }

        if (string.IsNullOrWhiteSpace(mediaUpload.Mime) || !IsSupportedMimeType(mediaUpload.Mime))
        {
            return Result.Failure<UserAvatar>(UserErrors.UserAvatarInvalidMimeType);
        }

        return Result.Success(new UserAvatar
        {
            Id = mediaUpload.BlobId,
            UserId = userId,
            Mime = mediaUpload.Mime,
            Url = mediaUpload.Url,
            Description = mediaUpload.Description ?? string.Empty
        });
    }

    public static bool IsSupportedMimeType(string contentType) => _supportedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
}
