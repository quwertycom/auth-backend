using Microsoft.AspNetCore.Mvc;
using API.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/token")]
[Produces("application/json")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public TokenController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateTokenAsync(string token)
    {
        // TODO: Use validation and create response and request models
        var result = await _tokenService.ValidateAsync(token);
        return Ok(result);
    }
}