using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Application.Users.UseCases.CreateUser;

namespace Y.Authentication.Presentation.Users;

[Route("api/user")]
public class UserController : ApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUseCaseHandler<CreateUserUseCase> _createUserUseCaseHandler;

    public UserController(
        ILogger<UserController> logger,
        IUseCaseHandler<CreateUserUseCase> createUserUseCaseHandler)
    {
        _logger = logger;
        _createUserUseCaseHandler = createUserUseCaseHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserUseCase request, CancellationToken cancellationToken)
    {
        var response = await _createUserUseCaseHandler.HandleAsync(request, cancellationToken);
        return response.IsFailure ? HandleFailure(response) : Ok("ok");
    }
}
