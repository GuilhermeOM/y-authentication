using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Domain.Services;
public interface IAuthService
{
    AuthToken CreateJwt(User user);
}
