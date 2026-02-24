using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Y.Authentication.Domain.Options;
using Y.Authentication.Infrastructure.Services;
using Y.Authentication.UnitTest.Fixtures;

namespace Y.Authentication.UnitTest.Services.Infrastructure;
public class AuthServiceTests
{
    private readonly Mock<IOptions<AuthOptions>> _authOptionsMock;

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _authOptionsMock = new Mock<IOptions<AuthOptions>>();

        _authOptionsMock
            .SetupGet(options => options.Value)
            .Returns(new AuthOptions
            {
                Secret = Guid.NewGuid().ToString(),
                Audience = "dummyAudience",
                Issuer = "dummyIssuer",
                ExpirationInMinutes = 60
            });

        _service = new AuthService(_authOptionsMock.Object);
    }

    [Fact]
    public void CreateJwt_ShouldThrowInvalidOperationException_WhenUserHasNoRoles()
    {
        // Arrange
        var user = UserFixture.CreateValid();
        user.Roles.Clear();

        // Act
        var action = () => _service.CreateJwt(user);

        // Assert
        action.Should().Throw<InvalidOperationException>("User must have at least one role");
    }

    [Fact]
    public void CreateJwt_ShouldReturnJwt()
    {
        // Arrange
        var user = UserFixture.CreateValidWithRoles();
        user.Verify();

        // Act
        var jwt = _service.CreateJwt(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(jwt.Token);
        var claims = jwtToken.Claims;

        claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
        claims.Should().Contain(claim => claim.Type == "verifiedAt" && claim.Value == user.VerifiedAt.ToString());

        foreach (var role in user.Roles)
        {
            claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == role.Role.Name);
        }

        jwt.Should().NotBeNull();
        jwt.TokenType.Should().Be("Bearer");
        jwt.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
