using FluentAssertions;
using Y.Authentication.Infrastructure.Services;

namespace Y.Authentication.UnitTest.Services.Infrastructure;
public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _service;

    public PasswordHasherServiceTests()
    {
        _service = new PasswordHasherService();
    }

    [Fact]
    public void HashPassword_ShouldHashPassword()
    {
        // Arrange
        var password = "testPassword123";

        // Act
        var result = _service.HashPassword(password);

        // Assert
        result.Should().NotBeNull();
        result.Salt.Should().NotBeEmpty();
        result.Hash.Should().NotBeEmpty();
    }

    [Fact]
    public void IsPasswordSequenceEqual_ShouldReturnTrue_WhenSequenceIsEqual()
    {
        // Arrange
        var password = "testPassword123";
        var passwordHash = _service.HashPassword(password);

        // Act
        var result = _service.IsPasswordSequenceEqual(password, passwordHash.Salt, passwordHash.Hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPasswordSequenceEqual_ShouldReturnFalse_WhenSequenceIsNotEqual()
    {
        // Arrange
        var password = "testPassword123";
        var wrongPassword = "wrongPassword456";
        var passwordHash = _service.HashPassword(password);

        // Act
        var result = _service.IsPasswordSequenceEqual(wrongPassword, passwordHash.Salt, passwordHash.Hash);

        // Assert
        result.Should().BeFalse();
    }
}
