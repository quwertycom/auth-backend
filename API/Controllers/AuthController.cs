using Microsoft.AspNetCore.Mvc;
using API.Service;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterUserAsync(string email, string password)
    {
        return Ok(await _authService.RegisterUserAsync(email, password));
    }
}