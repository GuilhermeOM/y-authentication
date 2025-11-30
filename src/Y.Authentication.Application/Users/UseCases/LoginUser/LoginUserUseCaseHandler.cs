using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services.Auth;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Users.UseCases.LoginUser;
internal sealed class LoginUserUseCaseHandler : IUseCaseHandler<LoginUserUseCase, AuthToken>
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public LoginUserUseCaseHandler(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthToken>> HandleAsync(LoginUserUseCase request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithMetadataRolesByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthToken>(UserErrors.UserNotFound);
        }

        if (!user.IsPasswordValid(request.Password))
        {
            return Result.Failure<AuthToken>(UserErrors.UserPasswordNotValid);
        }

        return user.VerifiedAt is null
            ? Result.Failure<AuthToken>(UserErrors.UserNotVerified)
            : Result.Success(_authService.CreateJwt(user));
    }
}
