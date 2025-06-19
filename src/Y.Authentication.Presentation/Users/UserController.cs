using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Application.Users.UseCases.Example;

namespace Y.Authentication.Presentation.Users;

[Route("api/user")]
public class UserController : ApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUseCaseHandler<ExampleUseCase, ExampleUseCaseResponse> _exampleUseCaseHandler;

    public UserController(
        ILogger<UserController> logger,
        IUseCaseHandler<ExampleUseCase, ExampleUseCaseResponse> exampleUseCaseHandler)
    {
        _logger = logger;
        _exampleUseCaseHandler = exampleUseCaseHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Example(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request at {EndpointName}", nameof(Example));

        var request = new ExampleUseCase("World");
        var response = await _exampleUseCaseHandler.HandleAsync(request, cancellationToken);

        if (response.IsFailure)
        {
            _logger.LogError("Error handling {EndpointName}: {@ErrorMessage}", nameof(Example), response.Error);
            return BadRequest(response.Error);
        }

        return Ok(response.Value);
    }
}
