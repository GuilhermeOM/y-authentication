using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Application.Users.UseCases.CreateUser;
using Y.Authentication.Application.Users.UseCases.VerifyUser;
using Y.Authentication.Domain.Constants;

namespace Y.Authentication.Presentation.Users;

[Route("api/user")]
public class UserController : ApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUseCaseHandler<CreateUserUseCase> _createUserUseCaseHandler;
    private readonly IUseCaseHandler<VerifyUserUseCase> _verifyUserUseCaseHandler;

    public UserController(
        ILogger<UserController> logger,
        IUseCaseHandler<CreateUserUseCase> createUserUseCaseHandler,
        IUseCaseHandler<VerifyUserUseCase> verifyUserUseCaseHandler)
    {
        _logger = logger;
        _createUserUseCaseHandler = createUserUseCaseHandler;
        _verifyUserUseCaseHandler = verifyUserUseCaseHandler;
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
}
