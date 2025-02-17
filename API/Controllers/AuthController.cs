using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Contracts.Responses.Auth;
using API.Contracts.Responses.Common;
using API.Contracts.Requests.Auth;
using API.Services.Interfaces;

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
        var response = await _authService.RegisterUserAsync(request);

        if (response.isSuccess)
        {
            if (response.status == "OTP_SENT" && response.verificationSessionID != null)
            {
                return Ok(new RegisterResponse { Status = "SUCCESS", Message = "OTP has been sent to your email. Please verify your email and login.", VerificationSessionID = response.verificationSessionID.Value });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later." });
            }
        }
        return BadRequest(new ErrorResponse { Status = response.status, Message = response.message });
    }

    [HttpPost("register/verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var response = await _authService.VerifyEmailAsync(request);

        if (response.isSuccess)
        {
            if (response.status == "SUCCESS")
            {
                return Ok(new VerifyEmailResponse { Status = response.status, Message = response.message, Email = request.Email });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later." });
            }
        }
        else
        {
            return BadRequest(new ErrorResponse { Status = response.status, Message = response.message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        Console.WriteLine(response.status);

        if (response.isSuccess)
        {
            if (response.status == "SUCCESS" && response.accessToken != null && response.refreshToken != null)
                return Ok(new LoginResponse { Status = response.status, Message = response.message, AccessToken = response.accessToken, RefreshToken = response.refreshToken });
            else
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later or contact support if the issue persists." });
        }
        else
        {
            if (response.status == "NOT_FOUND" || response.status == "INVALID_PASSWORD")
                return BadRequest(new ErrorResponse { Status = "INVALID_CREDENTIALS", Message = "Invalid credentials." });
            else
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later or contact support if the issue persists." });
        }
    }
}