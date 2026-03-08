using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Polly.Registry;
using Y.Authentication.Domain.Options;
using Y.Authentication.Domain.ValueObjects;
using Y.Authentication.Infrastructure.Resilience;
using Y.Authentication.Infrastructure.Services;

namespace Y.Authentication.UnitTest.Services.Infrastructure;

public class StorageServiceTests
{
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly Mock<ResiliencePipelineProvider<string>> _resiliencePipelineProviderMock;
    private readonly Mock<IOptions<BlobStorageOptions>> _blobStorageOptionsMock;
    private readonly Mock<BlobContainerClient> _blobContainerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;

    private readonly StorageService _service;

    public StorageServiceTests()
    {
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _resiliencePipelineProviderMock = new Mock<ResiliencePipelineProvider<string>>();
        _blobStorageOptionsMock = new Mock<IOptions<BlobStorageOptions>>();
        _blobContainerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();

        _resiliencePipelineProviderMock
           .Setup(mock => mock.GetPipeline(It.Is<string>(x => x == Resiliences.FastDefaultRetryPipelinePolicy)))
           .Returns(ResiliencePipeline.Empty);

        _blobStorageOptionsMock
            .SetupGet(mock => mock.Value)
            .Returns(new BlobStorageOptions
            {
                BaseUrl = "http://localhost:10000"
            });

        _blobServiceClientMock
            .Setup(client => client.GetBlobContainerClient(StorageService.PublicAuthenticationContainerName))
            .Returns(_blobContainerClientMock.Object);

        _service = new StorageService(
            _blobServiceClientMock.Object,
            _resiliencePipelineProviderMock.Object,
            _blobStorageOptionsMock.Object);
    }

    [Fact]
    public async Task UploadMediaAsync_ShouldReturnFailure_WhenBlobStorageUploadFails()
    {
        // Arrange
        var fileUpload = new FileUpload(
            Guid.NewGuid(),
            new MemoryStream([0x00, 0x01, 0x02]),
            "/file.jpg",
            "image/jpeg",
            "jpg");

        _blobServiceClientMock
            .Setup(mock => mock.GetBlobContainerClient(StorageService.PublicAuthenticationContainerName))
            .Returns(_blobContainerClientMock.Object);

        _blobContainerClientMock
            .Setup(mock => mock.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        var responseMock = new Mock<Response<BlobContentInfo>>();
        responseMock.SetupGet(mock => mock.GetRawResponse().Status).Returns(500);

        _blobClientMock
            .Setup(mock => mock.UploadAsync(
                fileUpload.Data,
                It.Is<BlobHttpHeaders>(x => x.ContentType == fileUpload.Mime && x.CacheControl == "public, max-age=31536000"),
                default,
                default,
                default,
                default,
                default,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _service.UploadAsync(fileUpload, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(StorageServiceErrors.BlobStorageFailure);
    }

    [Fact]
    public async Task UploadMediaAsync_ShouldSucceed()
    {
        // Arrange
        var fileUpload = new FileUpload(
            Guid.NewGuid(),
            new MemoryStream([0x00, 0x01, 0x02]),
            "file.jpg",
            "image/jpeg",
            "jpg");

        _blobServiceClientMock
            .Setup(mock => mock.GetBlobContainerClient(StorageService.PublicAuthenticationContainerName))
            .Returns(_blobContainerClientMock.Object);

        _blobContainerClientMock
            .Setup(mock => mock.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        var responseMock = new Mock<Response<BlobContentInfo>>();
        responseMock.SetupGet(mock => mock.GetRawResponse().Status).Returns(201);

        _blobClientMock
            .Setup(mock => mock.UploadAsync(
                fileUpload.Data,
                It.Is<BlobHttpHeaders>(x => x.ContentType == fileUpload.Mime && x.CacheControl == "public, max-age=31536000"),
                default,
                default,
                default,
                default,
                default,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _service.UploadAsync(fileUpload, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.BlobId.Should().Be(fileUpload.BlobId);
        result.Value.Path.Should().Be(fileUpload.Path);
        result.Value.Mime.Should().Be(fileUpload.Mime);
        result.Value.Description.Should().Be(fileUpload.Description);
        result.Value.Url.Should().Be($"{_blobStorageOptionsMock.Object.Value.BaseUrl}/{StorageService.PublicAuthenticationContainerName}/{fileUpload.Path}");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed()
    {
        // Arrange
        var dummyFilePath = "path/to/file.jpg";

        _blobContainerClientMock
            .Setup(client => client.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        // Act
        await _service.DeleteAsync(dummyFilePath);

        // Assert
        _blobClientMock
            .Verify(mock => mock.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, default));
    }
}
