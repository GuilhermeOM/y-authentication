using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Domain.Services;
public interface IStorageService
{
    Task<Result<FileUpload>> UploadAsync(
        Stream stream,
        FileInspectionResult inspectionResult,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(FileUpload media);
}
