using FluentAssertions;
using MassTransit;
using Moq;
using Y.Authentication.Application.Users.UseCases.VerifyUser;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;
using Y.Contract.Root.Core.Events;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class VerifyUserUseCaseTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly VerifyUserUseCaseHandler _handler;

    public VerifyUserUseCaseTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock
            .Setup(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new VerifyUserUseCaseHandler(
            _publishEndpointMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(User));

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserNotFound);

        _userRepositoryMock
            .Verify(mock => mock.VerifyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        _publishEndpointMock
            .Verify(mock => mock.Publish(It.IsAny<CreateProfileEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConflict_WhenUserAlreadyVerified()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "dummy@email.com",
            VerifiedAt = DateTime.UtcNow,
        };

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserAlreadyVerified);

        _userRepositoryMock
            .Verify(mock => mock.VerifyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        _publishEndpointMock
            .Verify(mock => mock.Publish(It.IsAny<CreateProfileEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyc_ShouldReturnVerificationFailed_WhenVerifyAsyncFails()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "dummy@email.com",
        };

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(mock => mock.VerifyAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserVerificationFailed);

        _publishEndpointMock
            .Verify(mock => mock.Publish(It.IsAny<CreateProfileEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyc_ShouldReturnSuccess_WhenVerifyAsyncSucceeds()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "dummy@email.com",
            Metadata = new UserMetadata
            {
                Name = "dummy",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow)
            }
        };

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(mock => mock.VerifyAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _publishEndpointMock
            .Setup(mock => mock.Publish(
                It.Is<CreateProfileEvent>(@event =>
                    @event.CorrelationId == user.Id.ToString()
                    && @event.UserId == user.Id
                    && @event.Name == user.Metadata.Name),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
