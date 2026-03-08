using Microsoft.AspNetCore.Http;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Application.Users.Services.CreateUserAvatar;
internal interface ICreateUserAvatarService
{
    Task<Result<FileUploadResult?>> UploadAsync(
        IFormFile? avatarPhoto,
        CancellationToken cancellationToken);

    Task RollbackUploadAsync(FileUploadResult? avatarUpload);
}
