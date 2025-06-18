using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Y.Authentication.Presentation.Users;

[Route("api/user")]
public class UserController : ApiController
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Example()
    {
        _logger.LogInformation("logging demo, {@Demo}", new { Id = "id", Name = "Name", Age = 1 });

        await Task.Delay(1000);
        return Ok("example");
    }
}
