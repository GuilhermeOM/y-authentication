using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Y.Authentication.Application.Users.UseCases.CreateUser;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;
using Y.Contract.Root.Notification.Events;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class CreateUseCaseHandlerTests
{
    private readonly Mock<ILogger<CreateUserUseCaseHandler>> _loggerMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserMetadataRepository> _userMetadataRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly CreateUserUseCaseHandler _handler;

    public CreateUseCaseHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateUserUseCaseHandler>>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userMetadataRepositoryMock = new Mock<IUserMetadataRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock
            .Setup(mock => mock.TransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<Result>>, CancellationToken>((func, _) => func());

        _handler = new CreateUserUseCaseHandler(
            _loggerMock.Object,
            _publishEndpointMock.Object,
            _userRepositoryMock.Object,
            _userMetadataRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserAlreadyExists()
    {
        // Arrange
        var request = CreateRequest();

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserAlreadyExists);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserNotCreated()
    {
        // Arrange
        var request = CreateRequest();

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(It.Is<User>(user => user.Email == request.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserCreationFailed);

        _publishEndpointMock
            .Verify(mock => mock.Publish(It.IsAny<SendEmailEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserMetadataNotCreated()
    {
        // Arrange
        var request = CreateRequest();

        var createdUserId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(It.Is<User>(user => user.Email == request.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserId);

        _userMetadataRepositoryMock
            .Setup(mock => mock.CreateAsync(It.Is<UserMetadata>(metadata => metadata.UserId == createdUserId && metadata.Name == request.Name), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserMetadataErrors.UserMetadataCreationFailed);

        _publishEndpointMock
            .Verify(mock => mock.Publish(It.IsAny<SendEmailEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateRequest();

        var createdUserId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(It.Is<User>(user => user.Email == request.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserId);

        _userMetadataRepositoryMock
            .Setup(mock => mock.CreateAsync(It.Is<UserMetadata>(metadata => metadata.UserId == createdUserId && metadata.Name == request.Name), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _publishEndpointMock
            .Verify(mock => mock.Publish(
                It.Is<SendEmailEvent>(@event => @event.CorrelationId == createdUserId.ToString() && @event.Email == request.Email),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    public static CreateUserUseCase CreateRequest() => new(
        "email@email.com",
        "password",
        "name",
        DateOnly.FromDateTime(DateTime.Now));
}
