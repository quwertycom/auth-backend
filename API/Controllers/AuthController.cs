using Microsoft.AspNetCore.Mvc;
using API.Service;
using API.Contracts.Responses.Auth;
using API.Contracts.Responses.Common;
using API.Contracts.Requests.Auth;

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
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUserAsync(RegisterRequest request)
    {
        var response = await _authService.RegisterUserAsync(request.Email, request.Password);

        if (response.isSuccess)
        {
            if (response.status == "SUCCESS" && response.verificationSessionID != null) {
                return Ok(new RegisterResponse { Status = response.status, Message = response.message, VerificationSessionID = response.verificationSessionID ?? 0 });
            }
        }
        return BadRequest(new ErrorResponse { Status = response.status, Message = response.message });
    }
}