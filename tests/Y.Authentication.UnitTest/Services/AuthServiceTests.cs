using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Services.Auth;
using Y.Authentication.Infrastructure.Services.Auth;

namespace Y.Authentication.UnitTest.Services;
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
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "dummyEmail",
        };

        // Act
        var action = () => _service.CreateJwt(user);

        // Assert
        action.Should().Throw<InvalidOperationException>("User must have at least one role");
    }

    [Fact]
    public void CreateJwt_ShouldReturnJwt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();
        var adminRoleId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "dummy@dummy.com",
            VerifiedAt = DateTime.UtcNow,
            Roles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = userRoleId,
                    Role = new Role
                    {
                        Id = userRoleId,
                        Name = Contract.Root.Authentication.Shared.Role.User.ToString()
                    }
                },
                new UserRole
                {
                    UserId = userId,
                    RoleId = adminRoleId,
                    Role = new Role
                    {
                        Id = adminRoleId,
                        Name = Contract.Root.Authentication.Shared.Role.Admin.ToString()
                    }
                }
            ]
        };

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
