using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Application.Users.UseCases.LoginUser;
internal sealed class LoginUserUseCaseHandler : IUseCaseHandler<LoginUserUseCase, AuthToken>
{
    private readonly IAuthService _authService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IUserRepository _userRepository;

    public LoginUserUseCaseHandler(
        IAuthService authService,
        IPasswordHasherService passwordHasherService,
        IUserRepository userRepository)
    {
        _authService = authService;
        _passwordHasherService = passwordHasherService;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthToken>> HandleAsync(LoginUserUseCase request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithMetadataAvatarRolesByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthToken>(UserErrors.UserNotFound);
        }

        var isPasswordCorrect = _passwordHasherService
            .IsPasswordSequenceEqual(request.Password, user.PasswordSalt, user.PasswordHash);

        if (!isPasswordCorrect)
        {
            return Result.Failure<AuthToken>(UserErrors.UserPasswordNotValid);
        }

        return user.VerifiedAt is null
            ? Result.Failure<AuthToken>(UserErrors.UserNotVerified)
            : Result.Success(_authService.CreateJwt(user));
    }
}
