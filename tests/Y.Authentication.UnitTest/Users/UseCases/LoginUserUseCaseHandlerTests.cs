using FluentAssertions;
using Moq;
using Y.Authentication.Application.Users.UseCases.LoginUser;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.ValueObjects;
using Y.Authentication.UnitTest.Fixtures;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class LoginUserUseCaseHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    private readonly LoginUserUseCaseHandler _handler;

    public LoginUserUseCaseHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _passwordHasherServiceMock = new Mock<IPasswordHasherService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new LoginUserUseCaseHandler(
            _authServiceMock.Object,
            _passwordHasherServiceMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummypassword");

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(User));

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        response.IsFailure.Should().BeTrue();
        response.Error.Should().BeEquivalentTo(UserErrors.UserNotFound);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUnauthorized_WhenUserPasswordNotValid()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummyWrongPass");
        var user = UserFixture.CreateValid();

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(mock => mock.IsPasswordSequenceEqual(request.Password, user.PasswordSalt, user.PasswordHash))
            .Returns(false);

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        response.IsFailure.Should().BeTrue();
        response.Error.Should().BeEquivalentTo(UserErrors.UserPasswordNotValid);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUnauthorized_WhenUserNotVerified()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummyCorrectPass");
        var user = UserFixture.CreateValid();

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(mock => mock.IsPasswordSequenceEqual(request.Password, user.PasswordSalt, user.PasswordHash))
            .Returns(true);

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        response.IsFailure.Should().BeTrue();
        response.Error.Should().BeEquivalentTo(UserErrors.UserNotVerified);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateJwt()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummyCorrectPass");
        var user = UserFixture.CreateValid();
        user.Verify();

        _userRepositoryMock
            .Setup(mock => mock.GetWithMetadataRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
           .Setup(mock => mock.IsPasswordSequenceEqual(request.Password, user.PasswordSalt, user.PasswordHash))
           .Returns(true);

        var authToken = new AuthToken("Bearer", "dummyJwt", DateTime.UtcNow.AddHours(1));

        _authServiceMock
            .Setup(mock => mock.CreateJwt(It.Is<User>(u => u.Id == user.Id && u.Email == user.Email)))
            .Returns(authToken);

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().BeEquivalentTo(authToken);
    }
}
