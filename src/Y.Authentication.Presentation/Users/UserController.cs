using Microsoft.AspNetCore.Mvc;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Application.Users.UseCases.CreateUser;
using Y.Authentication.Application.Users.UseCases.LoginUser;
using Y.Authentication.Application.Users.UseCases.VerifyUser;
using Y.Authentication.Domain.Constants;
using Y.Authentication.Domain.Services.Auth;

namespace Y.Authentication.Presentation.Users;

[Route("api/user")]
public class UserController : ApiController
{
    private readonly IUseCaseHandler<CreateUserUseCase> _createUserUseCaseHandler;
    private readonly IUseCaseHandler<VerifyUserUseCase> _verifyUserUseCaseHandler;
    private readonly IUseCaseHandler<LoginUserUseCase, AuthToken> _loginUserUseCaseHandler;

    public UserController(
        IUseCaseHandler<CreateUserUseCase> createUserUseCaseHandler,
        IUseCaseHandler<VerifyUserUseCase> verifyUserUseCaseHandler,
        IUseCaseHandler<LoginUserUseCase, AuthToken> loginUserUseCaseHandler)
    {
        _createUserUseCaseHandler = createUserUseCaseHandler;
        _verifyUserUseCaseHandler = verifyUserUseCaseHandler;
        _loginUserUseCaseHandler = loginUserUseCaseHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserUseCase request, CancellationToken cancellationToken)
    {
        var response = await _createUserUseCaseHandler.HandleAsync(request, cancellationToken);
        return response.IsFailure ? HandleFailure(response) : NoContent();
    }

    [HttpGet(UserConstants.VerifyEndpoint)]
    public async Task<IActionResult> VerifyAsync([FromQuery] VerifyUserUseCase request, CancellationToken cancellationToken)
    {
        var response = await _verifyUserUseCaseHandler.HandleAsync(request, cancellationToken);
        return response.IsFailure ? HandleFailure(response) : NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromQuery] LoginUserUseCase request, CancellationToken cancellationToken)
    {
        var response = await _loginUserUseCaseHandler.HandleAsync(request, cancellationToken);
        return response.IsFailure ? HandleFailure(response) : Ok(response.Value);
    }
}
