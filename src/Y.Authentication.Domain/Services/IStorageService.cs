using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Domain.Services;
public interface IStorageService
{
    Task<Result<FileUploadResult>> UploadAsync(
        FileUpload fileUpload,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string filePath);
}
