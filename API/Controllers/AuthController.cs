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
    // [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse { Status = "INVALID_REQUEST", Message = "Request cannot be null." });
        }

        var response = await _authService.RegisterUserAsync(request);

        if (response.isSuccess && response.status == "OTP_SENT" && response.verificationSessionID != null)
        {
            return Ok(new RegisterResponse
            {
                Status = "SUCCESS",
                Message = "OTP has been sent to your email. Please verify your email and login.",
                VerificationSessionID = response.verificationSessionID.Value
            });
        }

        return response.status switch
        {
            "INVALID_USERNAME" or "INVALID_PASSWORD" or "EMAIL_TAKEN" or "INVALID_EMAIL" or "INVALID_PHONE_NUMBER" or "PHONE_NUMBER_TAKEN" or "INVALID_BIRTHDATE" => BadRequest(new ErrorResponse
            {
                Status = response.status,
                Message = response.message
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Status = "INTERNAL_SERVER_ERROR",
                Message = response.message ?? "Something went wrong, please try again later."
            })
        };
    }

    [HttpPost("register/verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse { Status = "INVALID_REQUEST", Message = "Request cannot be null." });
        }

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
            if (response.status == "INVALID_VERIFICATION_CODE" || response.status == "VERIFICATION_SESSION_NOT_FOUND" || response.status == "EXPIRED" || response.status == "ALREADY_USED" || response.status == "INVALID_OTP")
            {
                return BadRequest(new ErrorResponse { Status = response.status, Message = response.message });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = response.message ?? "Something went wrong, please try again later." });
            }
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse { Status = "INVALID_REQUEST", Message = "Request cannot be null." });
        }

        var response = await _authService.LoginAsync(request);

        if (response.isSuccess)
        {
            if (response.status == "SUCCESS" && response.accessToken != null && response.refreshToken != null)
            {
                return Ok(new LoginResponse { Status = response.status, Message = response.message, AccessToken = response.accessToken, RefreshToken = response.refreshToken });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later." });
            }
        }
        else
        {
            if (response.status == "NOT_FOUND" || response.status == "INVALID_PASSWORD" || response.status == "ACCOUNT_LOCKED" || response.status == "USER_INACTIVE")
            {
                return BadRequest(new ErrorResponse { Status = response.status, Message = response.message });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later." });
            }
        }
    }
}