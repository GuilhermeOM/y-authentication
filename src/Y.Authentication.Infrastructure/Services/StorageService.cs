using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Polly.Registry;
using Y.Authentication.Domain.Options;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;
using Y.Authentication.Infrastructure.Resilience;

namespace Y.Authentication.Infrastructure.Services;
internal sealed class StorageService : IStorageService
{
    public const string PublicAuthenticationContainerName = "public-authentication";
    public const string ProfilePathName = "profiles";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
    private readonly IOptions<BlobStorageOptions> _blobStorageOptions;

    public StorageService(
        BlobServiceClient blobServiceClient,
        ResiliencePipelineProvider<string> resiliencePipelineProvider,
        IOptions<BlobStorageOptions> blobStorageOptions)
    {
        _blobServiceClient = blobServiceClient;
        _resiliencePipelineProvider = resiliencePipelineProvider;
        _blobStorageOptions = blobStorageOptions;
    }

    public async Task<Result<FileUpload>> UploadAsync(
        Stream stream,
        FileInspectionResult inspectionResult,
        CancellationToken cancellationToken = default)
    {
        return await _resiliencePipelineProvider
            .GetPipeline(Resiliences.FastDefaultRetryPipelinePolicy)
            .ExecuteAsync(async _ =>
            {
                return await UploadMediaAsync(
                    stream,
                    inspectionResult,
                    cancellationToken);
            }, cancellationToken);
    }

    private async Task<Result<FileUpload>> UploadMediaAsync(
        Stream data,
        FileInspectionResult inspectionResult,
        CancellationToken cancellationToken)
    {
        data.Seek(0, SeekOrigin.Begin);

        var blobId = Guid.NewGuid();
        var mediaPath = CreateMediaPath(blobId, inspectionResult.Extension);

        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(PublicAuthenticationContainerName);
        var blobClient = blobContainerClient.GetBlobClient(mediaPath);

        var upload = await blobClient.UploadAsync(data, new BlobHttpHeaders
        {
            ContentType = inspectionResult.Mime,
            CacheControl = "public, max-age=31536000"
        }, cancellationToken: cancellationToken);

        if (upload.GetRawResponse().Status >= (int)HttpStatusCode.BadRequest)
        {
            return Result.Failure<FileUpload>(StorageServiceErrors.BlobStorageFailure);
        }

        var mediaUpload = new FileUpload(
            blobId,
            CreateMediaPublicUrl(mediaPath),
            inspectionResult.Mime,
            inspectionResult.Extension);

        return Result.Success(mediaUpload);
    }

    public async Task DeleteAsync(FileUpload mediaUpload)
    {
        var mediaPath = CreateMediaPath(mediaUpload.BlobId, mediaUpload.Extension);

        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(PublicAuthenticationContainerName);
        var blobClient = blobContainerClient.GetBlobClient(mediaPath);

        await blobClient.DeleteIfExistsAsync();
    }

    private static string CreateMediaPath(Guid blobId, string extension) => $"{ProfilePathName}/{blobId:N}.{extension}";

    private string CreateMediaPublicUrl(string mediaPath) => $"{_blobStorageOptions.Value.BaseUrl}/{PublicAuthenticationContainerName}/{mediaPath}";
}

internal static class StorageServiceErrors
{
    public static Error BlobStorageFailure => new("BLOB_STORAGE_FAILURE", "An error occurred while communicating with blob storage");
}

