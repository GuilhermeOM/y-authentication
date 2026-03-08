using Microsoft.AspNetCore.Http;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Application.Users.Services.CreateUserAvatar;
internal sealed class CreateUserAvatarService : ICreateUserAvatarService
{
    public const string ProfilePathName = "profiles";

    private readonly IStorageService _storageService;
    private readonly IFileInspectorService _fileInspectorService;

    public CreateUserAvatarService(
        IStorageService storageService,
        IFileInspectorService fileInspectorService)
    {
        _storageService = storageService;
        _fileInspectorService = fileInspectorService;
    }

    public async Task<Result<FileUploadResult?>> UploadAsync(
        IFormFile? avatarPhoto,
        CancellationToken cancellationToken)
    {
        if (avatarPhoto is null)
        {
            return Result.Success(default(FileUploadResult));
        }

        using var stream = avatarPhoto.OpenReadStream();

        var inspectionResult = _fileInspectorService.InspectFileStream(stream);
        if (inspectionResult.IsFailure)
        {
            return Result.Failure<FileUploadResult?>(inspectionResult.Error);
        }

        if (!UserAvatar.IsSupportedMimeType(inspectionResult.Value.Mime))
        {
            return Result.Failure<FileUploadResult?>(UserErrors.UserAvatarUnsupportedMimeType);
        }

        var blobId = Guid.NewGuid();
        var mediaPath = CreateFilePath(blobId, inspectionResult.Value.Extension);

        var fileUpload = new FileUpload(
            blobId,
            stream,
            mediaPath,
            inspectionResult.Value.Mime,
            inspectionResult.Value.Extension);

        return (await _storageService.UploadAsync(fileUpload, cancellationToken))!;
    }

    private static string CreateFilePath(Guid blobId, string extension) => $"{ProfilePathName}/{blobId:N}.{extension}";

    public async Task RollbackUploadAsync(FileUploadResult? avatarUpload)
    {
        if (avatarUpload is null)
        {
            return;
        }

        await _storageService.DeleteAsync(avatarUpload.Path);
    }
}
