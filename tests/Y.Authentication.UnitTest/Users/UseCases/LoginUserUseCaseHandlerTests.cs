using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Moq;
using Y.Authentication.Application.Users.UseCases.LoginUser;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services.Auth;

namespace Y.Authentication.UnitTest.Users.UseCases;
public class LoginUserUseCaseHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    private readonly LoginUserUseCaseHandler _handler;

    public LoginUserUseCaseHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new LoginUserUseCaseHandler(_authServiceMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummypassword");

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
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

        CreatePasswordHash("dummyCorrectPass", out var passwordHash, out var passwordSalt);

        var user =  new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
        };

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        user.IsPasswordValid(request.Password).Should().BeFalse();

        response.IsFailure.Should().BeTrue();
        response.Error.Should().BeEquivalentTo(UserErrors.UserPasswordNotValid);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUnauthorized_WhenUserNotVerified()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummyCorrectPass");

        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
        };

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        user.IsPasswordValid(request.Password).Should().BeTrue();

        response.IsFailure.Should().BeTrue();
        response.Error.Should().BeEquivalentTo(UserErrors.UserNotVerified);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateJwt()
    {
        // Arrange
        var request = new LoginUserUseCase("dummy@dummy.com", "dummyCorrectPass");

        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            VerifiedAt = DateTime.UtcNow,
            Roles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Role = new Role
                    {
                        Id = roleId,
                        Name = Contract.Root.Authentication.Shared.Role.User.ToString()
                    }
                }
            ]
        };

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var authToken = new AuthToken("Bearer", "dummyJwt", DateTime.UtcNow.AddHours(1));

        _authServiceMock
            .Setup(mock => mock.CreateJwt(It.Is<User>(u => u.Id == user.Id && u.Email == user.Email)))
            .Returns(authToken);

        // Act
        var response = await _handler.HandleAsync(request, default);

        // Assert
        user.IsPasswordValid(request.Password).Should().BeTrue();

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().BeEquivalentTo(authToken);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
