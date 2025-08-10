using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Y.Authentication.Application.Users.DomainEvents.CreateUserRole;
using Y.Authentication.Application.Users.DomainEvents.CreateUserRole.Exceptions;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.UnitTest.Users.DomainEvents;
public class CreateUserRoleDomainEventHandlerTests
{
    private readonly Mock<ILogger<CreateUserRoleDomainEventHandler>> _loggerMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly CreateUserRoleDomainEventHandler _handler;

    public CreateUserRoleDomainEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateUserRoleDomainEventHandler>>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateUserRoleDomainEventHandler(
            _loggerMock.Object,
            _userRoleRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturn_WhenUserAlreadyHasRole()
    {
        // Arrange
        var domainEvent = new CreateUserRoleDomainEvent(Guid.NewGuid(), Contract.Root.Authentication.Shared.Role.User);
        var existingRoleId = Guid.NewGuid();

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(domainEvent.Role.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = existingRoleId, Name = domainEvent.Role.ToString() });

        _userRoleRepositoryMock
            .Setup(mock => mock.ExistsAsync(domainEvent.UserId, existingRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent, default);

        // Assert
        _roleRepositoryMock.Verify(mock => mock.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);

        _userRoleRepositoryMock.Verify(mock => mock.CreateAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowRoleNotCreatedException_WhenRoleNotCreated()
    {
        // Arrange
        var domainEvent = new CreateUserRoleDomainEvent(Guid.NewGuid(), Contract.Root.Authentication.Shared.Role.User);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(domainEvent.Role.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Role));

        _roleRepositoryMock
            .Setup(mock => mock.CreateAsync(It.Is<Role>(role => role.Name == domainEvent.Role.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        // Act
        var action = () => _handler.HandleAsync(domainEvent, default);

        // Assert
        await action.Should().ThrowAsync<RoleNotCreatedException>();

        _userRoleRepositoryMock.Verify(mock => mock.CreateAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowUserRoleNotCreatedException_WhenUserRoleNotCreated()
    {
        // Arrange
        var domainEvent = new CreateUserRoleDomainEvent(Guid.NewGuid(), Contract.Root.Authentication.Shared.Role.User);
        var existingRoleId = Guid.NewGuid();

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(domainEvent.Role.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role
            {
                Id = existingRoleId,
                Name = domainEvent.Role.ToString()
            });

        _userRoleRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<UserRole>(ur => ur.UserId == domainEvent.UserId && ur.RoleId == existingRoleId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        // Act
        var action = () => _handler.HandleAsync(domainEvent, default);

        // Assert
        await action.Should().ThrowAsync<UserRoleNotCreatedException>();

        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateUserRole_WhenRoleDoesNotExist()
    {
        // Arrange
        var domainEvent = new CreateUserRoleDomainEvent(Guid.NewGuid(), Contract.Root.Authentication.Shared.Role.User);

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(domainEvent.Role.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Role));

        var createdRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = domainEvent.Role.ToString()
        };

        _roleRepositoryMock
            .Setup(repo => repo.CreateAsync(It.Is<Role>(userRole => userRole.Name == domainEvent.Role.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRole.Id);

        var createdUserRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = domainEvent.UserId,
            RoleId = createdRole.Id
        };

        _userRoleRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<UserRole>(ur => ur.UserId == createdUserRole.UserId && ur.RoleId == createdUserRole.RoleId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserRole.Id);

        // Act
        await _handler.HandleAsync(domainEvent, default);

        // Assert
        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_ShoulCreateUserRole_WhenRoleExists()
    {
        // Arrange
        var domainEvent = new CreateUserRoleDomainEvent(Guid.NewGuid(), Contract.Root.Authentication.Shared.Role.User);

        var existingRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = domainEvent.Role.ToString()
        };

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(domainEvent.Role.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        var createdUserRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = domainEvent.UserId,
            RoleId = existingRole.Id
        };

        _userRoleRepositoryMock
            .Setup(mock => mock.CreateAsync(
                It.Is<UserRole>(ur => ur.UserId == createdUserRole.UserId && ur.RoleId == createdUserRole.RoleId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserRole.Id);

        // Act
        await _handler.HandleAsync(domainEvent, default);

        // Assert
        _roleRepositoryMock.Verify(mock => mock.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
