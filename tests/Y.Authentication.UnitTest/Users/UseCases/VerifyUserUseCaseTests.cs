using FluentAssertions;
using Moq;
using Y.Authentication.Application.Users.UseCases.VerifyUser;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.UnitTest.Fixtures;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class VerifyUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDomainEventsDispatcher> _domainEventsDispatcherMock;

    private readonly VerifyUserUseCaseHandler _handler;

    public VerifyUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _domainEventsDispatcherMock = new Mock<IDomainEventsDispatcher>();

        _unitOfWorkMock
            .Setup(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new VerifyUserUseCaseHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _domainEventsDispatcherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");

        _userRepositoryMock
            .Setup(mock => mock.TrackByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(User));

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserNotFound);

        _userRepositoryMock
            .Verify(mock => mock.TrackByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConflict_WhenUserAlreadyVerified()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");
        var user = UserFixture.CreateValid();
        user.Verify();

        _userRepositoryMock
            .Setup(mock => mock.TrackByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(UserErrors.UserAlreadyVerified);

        _userRepositoryMock
            .Verify(mock => mock.TrackByVerificationTokenAsync(user.VerificationToken, It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyc_ShouldReturnSuccess_WhenVerifyAsyncSucceeds()
    {
        // Arrange
        var request = new VerifyUserUseCase("dummy");
        var user = UserFixture.CreateValid();

        _userRepositoryMock
            .Setup(mock => mock.TrackByVerificationTokenAsync(request.VerificationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _domainEventsDispatcherMock
            .Setup(mock => mock.DispatchAsync(
                It.Is<List<IDomainEvent>>(events => AreDomainEventsWellDispatched(events, user)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock
            .Verify(mock => mock.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        user.VerifiedAt.Should().NotBeNull();
    }

    private static bool AreDomainEventsWellDispatched(List<IDomainEvent> events, User user)
    {
        var createUserProfileDomainEvent = events.OfType<CreateUserProfileDomainEvent>().FirstOrDefault();

        return events.Count == 1
            && createUserProfileDomainEvent?.UserId == user.Id
            && createUserProfileDomainEvent?.UserName == (user.Metadata?.Name ?? string.Empty);
    }
}
