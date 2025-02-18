using Microsoft.AspNetCore.Mvc;
using API.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/auth/authorization")]
[Produces("application/json")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateTokenAsync(string token)
    {
        // TODO: Use validation and create response and request models
        var result = await _authorizationService.ValidateTokenAsync(token);
        return Ok(result);
    }
}