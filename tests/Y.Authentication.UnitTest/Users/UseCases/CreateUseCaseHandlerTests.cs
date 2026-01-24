using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Y.Authentication.Application.Users.UseCases.CreateUser;
using Y.Authentication.Domain.Aggregates.Role;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services;
using Y.Contract.SharedKernel.Enums;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class CreateUseCaseHandlerTests
{
    private readonly Mock<ILogger<CreateUserUseCaseHandler>> _loggerMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDomainEventsDispatcher> _domainEventsDispatcherMock;

    private readonly CreateUserUseCaseHandler _handler;

    public CreateUseCaseHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateUserUseCaseHandler>>();
        _passwordHasherServiceMock = new Mock<IPasswordHasherService>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _domainEventsDispatcherMock = new Mock<IDomainEventsDispatcher>();

        _unitOfWorkMock
            .Setup(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateUserUseCaseHandler(
            _loggerMock.Object,
            _passwordHasherServiceMock.Object,
            _roleRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _domainEventsDispatcherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserAlreadyExists()
    {
        // Arrange
        var request = CreateRequest();
        var userRoleName = Contract.SharedKernel.Enums.Role.User.ToString();

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
    public async Task HandleAsync_ShouldReturnFailure_WhenUserRoleNotFound()
    {
        // Arrange
        var request = CreateRequest();
        var userRoleName = Contract.SharedKernel.Enums.Role.User.ToString();

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(
                It.Is<string>(roleName => roleName == userRoleName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Domain.Aggregates.Role.Role));

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserRoleNotFound);

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
        var userRoleName = Contract.SharedKernel.Enums.Role.User.ToString();
        var byteArrayMock = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(
                It.Is<string>(roleName => roleName == userRoleName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Aggregates.Role.Role.Create(userRoleName).Value);

        _passwordHasherServiceMock
            .Setup(mock => mock.HashPassword(request.Password))
            .Returns((byteArrayMock, byteArrayMock));

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<User>(user => user.Email == request.Email),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        var action = async () => await _handler.HandleAsync(request, default);

        // Assert
        await action.Should().ThrowExactlyAsync<Exception>();

        _userRepositoryMock.Verify(mock => mock.CreateMetadataAsync(
            It.IsAny<UserMetadata>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _userRepositoryMock.Verify(mock => mock.CreateRoleAsync(
            It.IsAny<UserRole>(),
            It.IsAny<CancellationToken>()), Times.Never);

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
        var userRoleName = Contract.SharedKernel.Enums.Role.User.ToString();
        var byteArrayMock = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(
                It.Is<string>(roleName => roleName == userRoleName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Aggregates.Role.Role.Create(userRoleName).Value);

        _passwordHasherServiceMock
            .Setup(mock => mock.HashPassword(request.Password))
            .Returns((byteArrayMock, byteArrayMock));

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<User>(user => user.Email == request.Email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _userRepositoryMock
            .Setup(mock => mock.CreateMetadataAsync(
                It.Is<UserMetadata>(metadata => metadata.UserId != Guid.Empty && metadata.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        var action = async () => await _handler.HandleAsync(request, default);

        // Assert
        await action.Should().ThrowAsync<Exception>();

        _userRepositoryMock.Verify(mock => mock.CreateRoleAsync(
            It.IsAny<UserRole>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _domainEventsDispatcherMock.Verify(mock => mock.DispatchAsync(
            It.IsAny<IEnumerable<IDomainEvent>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserRoleNotCreated()
    {
        // Arrange
        var request = CreateRequest();
        var userRoleName = Contract.SharedKernel.Enums.Role.User.ToString();
        var byteArrayMock = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(
                It.Is<string>(roleName => roleName == userRoleName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Aggregates.Role.Role.Create(userRoleName).Value);

        _passwordHasherServiceMock
            .Setup(mock => mock.HashPassword(request.Password))
            .Returns((byteArrayMock, byteArrayMock));

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<User>(user => user.Email == request.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _userRepositoryMock
            .Setup(mock => mock.CreateMetadataAsync(
                It.Is<UserMetadata>(metadata => metadata.UserId != Guid.Empty && metadata.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _userRepositoryMock
            .Setup(mock => mock.CreateRoleAsync(
                It.Is<UserRole>(role => role.UserId != Guid.Empty),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        var action = async () => await _handler.HandleAsync(request, default);

        // Assert
        await action.Should().ThrowAsync<Exception>();

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
        var byteArrayMock = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());

        var createUserMetadataId = Guid.NewGuid();
        var userRole = Domain.Aggregates.Role.Role.Create(Contract.SharedKernel.Enums.Role.User.ToString()).Value;

        _userRepositoryMock
            .Setup(mock => mock.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(
                It.Is<string>(roleName => roleName == userRole.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _passwordHasherServiceMock
            .Setup(mock => mock.HashPassword(request.Password))
            .Returns((byteArrayMock, byteArrayMock));

        _userRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<User>(user => user.Email == request.Email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _userRepositoryMock
            .Setup(mock => mock.CreateMetadataAsync(
                It.Is<UserMetadata>(metadata => metadata.UserId != Guid.Empty && metadata.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _userRepositoryMock
            .Setup(mock => mock.CreateRoleAsync(
                It.Is<UserRole>(role => role.UserId != Guid.Empty && role.Id == userRole.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole.Id);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _domainEventsDispatcherMock.Verify(mock => mock.DispatchAsync(
            It.Is<IEnumerable<IDomainEvent>>(domainEvents => AreDomainEventsWellDispatched(domainEvents, request)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static bool AreDomainEventsWellDispatched(IEnumerable<IDomainEvent> events, CreateUserUseCase request)
    {
        var sendUserEmailVerificationDomainEvent = events.OfType<SendUserEmailVerificationDomainEvent>().FirstOrDefault();

        return sendUserEmailVerificationDomainEvent?.Email == request.Email
            && sendUserEmailVerificationDomainEvent?.UserName == request.Name
            && sendUserEmailVerificationDomainEvent?.UserId != Guid.Empty
            && !string.IsNullOrWhiteSpace(sendUserEmailVerificationDomainEvent?.VerificationToken);
    }

    public static CreateUserUseCase CreateRequest() => new()
    {
        Email = "email@email.com",
        Password = "password",
        Name = "name",
        BirthDate = DateOnly.FromDateTime(DateTime.Now)
    };
}
