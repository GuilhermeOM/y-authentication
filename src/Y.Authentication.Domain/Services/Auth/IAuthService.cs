using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Domain.Services.Auth;
public interface IAuthService
{
    AuthToken CreateJwt(User user);
}
