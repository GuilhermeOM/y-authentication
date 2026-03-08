using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Y.Authentication.Application.Users.Services.CreateUserAvatar;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.UnitTest.Services.Application;
public class CreateUserAvatarServiceTests
{
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IFileInspectorService> _fileInspectorServiceMock;

    private readonly Mock<IFormFile> _fileMock;

    private readonly CreateUserAvatarService _service;

    public CreateUserAvatarServiceTests()
    {
        _storageServiceMock = new Mock<IStorageService>();
        _fileInspectorServiceMock = new Mock<IFileInspectorService>();

        _fileMock = new Mock<IFormFile>();

        var fileData = "file";
        var fileByteArray = Encoding.UTF8.GetBytes(fileData);
        var fileStream = new MemoryStream(fileByteArray);

        _fileMock.Setup(mock => mock.OpenReadStream()).Returns(fileStream);

        _service = new CreateUserAvatarService(
            _storageServiceMock.Object,
            _fileInspectorServiceMock.Object);
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnSucess_WhenAvatarPhotoIsNull()
    {
        // Act
        var result = await _service.UploadAsync(null, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnFailure_WhenFileInspectionFails()
    {
        // Arrange
        var expectedError = new Error("error", "error desc");

        using var fileStream = _fileMock.Object.OpenReadStream();

        _fileInspectorServiceMock
            .Setup(mock => mock.InspectFileStream(It.Is<Stream>(stream => stream == fileStream)))
            .Returns(Result.Failure<FileInspectionResult>(expectedError));

        // Act
        var result = await _service.UploadAsync(_fileMock.Object, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnFailure_WhenMimeTypeNotSupported()
    {
        // Arrange
        using var fileStream = _fileMock.Object.OpenReadStream();

        _fileInspectorServiceMock
            .Setup(mock => mock.InspectFileStream(It.Is<Stream>(stream => stream == fileStream)))
            .Returns(Result.Success(new FileInspectionResult("mime", ".mime")));

        // Act
        var result = await _service.UploadAsync(_fileMock.Object, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(UserErrors.UserAvatarUnsupportedMimeType);
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnStorageServiceResult()
    {
        // Arrange
        using var fileStream = _fileMock.Object.OpenReadStream();

        var fileUpload = new FileUpload(
            Guid.NewGuid(),
            fileStream,
            "path",
            "image/png",
            "png");

        var expectedFileUploadResult = new FileUploadResult(
            fileUpload.BlobId,
            "https://example.com/avatar.png",
            fileUpload.Path,
            fileUpload.Mime,
            string.Empty);

        var inspectionResult = new FileInspectionResult("image/jpeg", ".jpg");

        _fileInspectorServiceMock
            .Setup(mock => mock.InspectFileStream(It.Is<Stream>(stream => stream == fileUpload.Data)))
            .Returns(Result.Success(inspectionResult));

        _storageServiceMock
            .Setup(mock => mock.UploadAsync(
                It.Is<FileUpload>(fu => fu.Data == fileUpload.Data),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedFileUploadResult));

        // Act
        var result = await _service.UploadAsync(_fileMock.Object, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedFileUploadResult);
    }

    [Fact]
    public async Task RollbackUploadAsync_ShouldStop_WhenAvatarUploadIsNull()
    {
        // Arrange
        FileUploadResult? avatarUpload = null;

        // Act
        await _service.RollbackUploadAsync(avatarUpload);

        // Assert
        _storageServiceMock
            .Verify(mock => mock.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RollbackUploadAsync_ShouldDeleteUpload()
    {
        // Arrange
        var avatarUpload = new FileUploadResult(
            Guid.NewGuid(),
            "https://example.com/avatar.png",
            "/path",
            "image/jpeg",
            string.Empty);

        // Act
        await _service.RollbackUploadAsync(avatarUpload);

        // Assert
        _storageServiceMock
            .Verify(mock => mock.DeleteAsync(avatarUpload.Path), Times.Once);
    }
}
