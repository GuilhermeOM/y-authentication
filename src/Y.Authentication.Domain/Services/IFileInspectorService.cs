using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Domain.Services;

public interface IFileInspectorService
{
    Result<FileInspectionResult> InspectFileStream(Stream stream);
}
