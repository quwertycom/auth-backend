using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login()
    {
        return Ok();
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register()
    {
        return Ok();
    }
}