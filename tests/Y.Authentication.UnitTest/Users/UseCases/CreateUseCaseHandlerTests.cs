using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Y.Authentication.Application.Users.UseCases.CreateUser;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Contract.Root.Notification.Events;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class CreateUseCaseHandlerTests
{
    private readonly Mock<ILogger<CreateUserUseCaseHandler>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDomainEventsDispatcher> _domainEventsDispatcherMock;

    private readonly CreateUserUseCaseHandler _handler;

    public CreateUseCaseHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateUserUseCaseHandler>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userMetadataRepositoryMock = new Mock<IUserMetadataRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _domainEventsDispatcherMock = new Mock<IDomainEventsDispatcher>();

        _unitOfWorkMock
            .Setup(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateUserUseCaseHandler(
            _loggerMock.Object,
            _userRepositoryMock.Object,
            _userMetadataRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _domainEventsDispatcherMock.Object);
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

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _domainEventsDispatcherMock.Verify(mock => mock.DispatchAsync(
            It.IsAny<IEnumerable<IDomainEvent>>(),
            It.IsAny<CancellationToken>()), Times.Never);
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

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _domainEventsDispatcherMock.Verify(mock => mock.DispatchAsync(
            It.IsAny<IEnumerable<IDomainEvent>>(),
            It.IsAny<CancellationToken>()), Times.Never);
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

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _domainEventsDispatcherMock.Verify(mock => mock.DispatchAsync(
            It.IsAny<IEnumerable<IDomainEvent>>(),
            It.IsAny<CancellationToken>()), Times.Never);
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

        var payload = It.IsAny<SendEmailEvent>();

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _domainEventsDispatcherMock.Verify(mock => mock.DispatchAsync(
            It.Is<IEnumerable<IDomainEvent>>(domainEvents => AreDomainEventsValid(domainEvents, createdUserId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    public static bool AreDomainEventsValid(IEnumerable<IDomainEvent> domainEvents, Guid userId)
    {
        var domainEventsAmount = domainEvents.Count();
        if (domainEventsAmount != 2)
        {
            return false;
        }

        if (domainEvents.First() is not CreateUserRoleDomainEvent createUserRoleDomainEvent
            || domainEvents.Last() is not SendUserEmailVerificationDomainEvent sendUserEmailVerificationDomainEvent)
        {
            return false;
        }

        return createUserRoleDomainEvent.UserId == userId
            && createUserRoleDomainEvent.Role == Contract.Root.Authentication.Shared.Role.User
            && sendUserEmailVerificationDomainEvent.UserId == userId
            && sendUserEmailVerificationDomainEvent.Email is not null
            && sendUserEmailVerificationDomainEvent.UserName is not null
            && sendUserEmailVerificationDomainEvent.VerificationToken is not null;
    }

    public static CreateUserUseCase CreateRequest() => new()
    {
        Email = "email@email.com",
        Password = "password",
        Name = "name",
        BirthDate = DateOnly.FromDateTime(DateTime.Now)
    };
}
