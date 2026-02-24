using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Internal;
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

    private readonly CreateUserAvatarService _service;

    public CreateUserAvatarServiceTests()
    {
        _storageServiceMock = new Mock<IStorageService>();
        _fileInspectorServiceMock = new Mock<IFileInspectorService>();

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

        var avatarPhoto = new FormFile(
            baseStream: new MemoryStream(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString())),
            baseStreamOffset: 0,
            length: 1,
            name: "avatarPhoto",
            fileName: "avatarPhoto.jpg");

        using var avatarPhotoStream = avatarPhoto.OpenReadStream();

        _fileInspectorServiceMock
            .Setup(mock => mock.InspectFileStream(It.Is<Stream>(stream => stream.Length == avatarPhotoStream.Length)))
            .Returns(Result.Failure<FileInspectionResult>(expectedError));

        // Act
        var result = await _service.UploadAsync(avatarPhoto, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnFailure_WhenMimeTypeNotSupported()
    {
        // Arrange
        var avatarPhoto = new FormFile(
            baseStream: new MemoryStream(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString())),
            baseStreamOffset: 0,
            length: 1,
            name: "avatarPhoto",
            fileName: "avatarPhoto.jpg");

        using var avatarPhotoStream = avatarPhoto.OpenReadStream();

        _fileInspectorServiceMock
            .Setup(mock => mock.InspectFileStream(It.Is<Stream>(stream => stream.Length == avatarPhotoStream.Length)))
            .Returns(Result.Success(new FileInspectionResult("mime", ".mime")));

        // Act
        var result = await _service.UploadAsync(avatarPhoto, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(UserErrors.UserAvatarUnsupportedMimeType);
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnStorageServiceResult()
    {
        // Arrange
        var expectedFileUpload = new FileUpload(
            blobId: Guid.NewGuid(),
            url: "https://example.com/avatar.png",
            mime: "image/png",
            extension: ".png");

        var avatarPhoto = new FormFile(
            baseStream: new MemoryStream(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString())),
            baseStreamOffset: 0,
            length: 1,
            name: "avatarPhoto",
            fileName: "avatarPhoto.jpg");

        using var avatarPhotoStream = avatarPhoto.OpenReadStream();

        var inspectionResult = new FileInspectionResult("image/jpeg", ".jpg");

        _fileInspectorServiceMock
            .Setup(mock => mock.InspectFileStream(It.Is<Stream>(stream => stream.Length == avatarPhotoStream.Length)))
            .Returns(Result.Success(inspectionResult));

        _storageServiceMock
            .Setup(mock => mock.UploadAsync(
                It.Is<Stream>(stream => stream.Length == avatarPhotoStream.Length),
                inspectionResult,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedFileUpload));

        // Act
        var result = await _service.UploadAsync(avatarPhoto, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedFileUpload);
    }

    [Fact]
    public async Task RollbackUploadAsync_ShouldStop_WhenAvatarUploadIsNull()
    {
        // Arrange
        FileUpload? avatarUpload = null;

        // Act
        await _service.RollbackUploadAsync(avatarUpload);

        // Assert
        _storageServiceMock
            .Verify(mock => mock.DeleteAsync(It.IsAny<FileUpload>()), Times.Never);
    }

    [Fact]
    public async Task RollbackUploadAsync_ShouldDeleteUpload()
    {
        // Arrange
        var avatarUpload = new FileUpload(
            blobId: Guid.NewGuid(),
            url: "https://example.com/avatar.png",
            mime: "image/png",
            extension: ".png");

        // Act
        await _service.RollbackUploadAsync(avatarUpload);

        // Assert
        _storageServiceMock
            .Verify(mock => mock.DeleteAsync(avatarUpload), Times.Once);
    }
}
