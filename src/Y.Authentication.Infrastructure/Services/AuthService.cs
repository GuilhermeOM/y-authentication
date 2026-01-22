using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Options;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Infrastructure.Services;
internal sealed class AuthService : IAuthService
{
    private readonly AuthOptions _authOptions;

    public AuthService(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }

    public AuthToken CreateJwt(User user)
    {
        if (user.Roles.Count == 0)
        {
            throw new InvalidOperationException("User must contain at least one role in order to generate a token");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Metadata?.Name ?? "Unknown"),
            new(JwtRegisteredClaimNames.Birthdate, user.Metadata?.BirthDate.ToString("yyyy-MM-dd") ?? string.Empty), 
            new("verifiedAt", user.VerifiedAt.ToString() ?? string.Empty)
        };

        claims.AddRange(user.Roles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name)));

        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_authOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _authOptions.Issuer,
            Audience = _authOptions.Audience,
            TokenType = "Bearer"
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescription);

        return new AuthToken(tokenDescription.TokenType, token, tokenDescription.Expires.Value);
    }
}
