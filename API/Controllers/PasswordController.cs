using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Contracts.Requests.Password;
using API.Contracts.Responses.Password;
using API.Contracts.Responses.Common;
using API.Services.Interfaces;
namespace API.Controllers
{
    [ApiController]
    [Route("api/password")]
    public class PasswordController : Controller
    {
        private readonly IPasswordService _passwordService;
        public PasswordController(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        [HttpPost("request-reset")]
        [ProducesResponseType(typeof(RequestResetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestReset([FromBody] RequestResetRequest request)
        {
            if (request.Email != null && request.Email != "")
            {
                Console.WriteLine("Requesting reset via email: " + request.Email);
                var response = await _passwordService.RequestResetViaEmail(request.Email);
                Console.WriteLine("Response: " + response.message);
                if (response.isSuccess)
                {
                    return Ok(new RequestResetResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
                else
                {
                    return BadRequest(new ErrorResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
            }
            else if (request.Username != null && request.Username != "")
            {
                var response = await _passwordService.RequestResetViaUsername(request.Username);
                if (response.isSuccess)
                {
                    return Ok(new RequestResetResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
                else
                {
                    return BadRequest(new ErrorResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
            }
            else
            {
                return BadRequest(new ErrorResponse
                {
                    Status = "error",
                    Message = "Email or username is required"
                });
            }
        }


        [HttpPost("validate-reset-code")]
        [ProducesResponseType(typeof(ValidateResetCodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateResetCode([FromBody] ValidateResetCodeRequest request)
        {
            if (request.Code.Length != 32)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = "error",
                    Message = "Invalid reset code"
                });
            }
            else
            {
                var response = await _passwordService.ValidateResetCode(request.Code);
                if (response.isSuccess)
                {
                    return Ok(new ValidateResetCodeResponse
                    {
                        Status = response.status,
                        Message = response.message,
                        IsValid = response.isValid
                    });
                }
                else
                {
                    if (response.status == "INTERNAL_ERROR")
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                        {
                            Status = response.status,
                            Message = response.message ?? "Internal server error, please try again later, if issue persists contact support."
                        });
                    }
                    else
                    {
                        return BadRequest(new ErrorResponse
                        {
                            Status = response.status,
                            Message = response.message
                        });
                    }
                }
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword()
        {
            return Ok();
        }
    }
}
