using Microsoft.AspNetCore.Mvc;

namespace Y.Authentication.Presentation.Users;

[Route("api/user")]
public class UserController : ApiController
{
    [HttpGet]
    public async Task<IActionResult> Example()
    {
        await Task.Delay(1000);
        return Ok("example");
    }
}
