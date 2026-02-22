using Microsoft.AspNetCore.Http;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Application.Users.Services.CreateUserAvatar;
internal sealed class CreateUserAvatarService : ICreateUserAvatarService
{
    private readonly IStorageService _storageService;
    private readonly IFileInspectorService _fileInspectorService;

    public CreateUserAvatarService(
        IStorageService storageService,
        IFileInspectorService fileInspectorService)
    {
        _storageService = storageService;
        _fileInspectorService = fileInspectorService;
    }

    public async Task<Result<FileUpload?>> UploadAsync(
        IFormFile? avatarPhoto,
        CancellationToken cancellationToken)
    {
        if (avatarPhoto is null)
        {
            return Result.Success(default(FileUpload));
        }

        using var stream = avatarPhoto.OpenReadStream();

        var inspectionResult = _fileInspectorService.InspectFileStream(stream);
        if (inspectionResult.IsFailure)
        {
            return Result.Failure<FileUpload?>(UserErrors.UserAvatarInspectionFailed);
        }

        if (!UserAvatar.IsSupportedMimeType(inspectionResult.Value.Mime))
        {
            return Result.Failure<FileUpload?>(UserErrors.UserAvatarUnsupportedMimeType);
        }

        var uploadResult = await _storageService
            .UploadAsync(stream, inspectionResult.Value, cancellationToken);

        return uploadResult!;
    }

    public async Task RollbackUploadAsync(FileUpload? mediaUpload)
    {
        if (mediaUpload is null)
        {
            return;
        }

        await _storageService.DeleteAsync(mediaUpload);
    }
}
