using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Contracts;
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
        [ProducesResponseType(typeof(Contracts.Responses.Password.RequestResetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestReset([FromBody] Contracts.Requests.Password.RequestResetRequest request)
        {
            if (request.Email != null && request.Email != "")
            {
                Console.WriteLine("Requesting reset via email: " + request.Email);
                var response = await _passwordService.RequestResetViaEmail(request.Email);
                Console.WriteLine("Response: " + response.message);
                if (response.isSuccess)
                {
                    return Ok(new Contracts.Responses.Password.RequestResetResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
                else
                {
                    return BadRequest(new Contracts.Responses.Common.ErrorResponse
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
                    return Ok(new Contracts.Responses.Password.RequestResetResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
                else
                {
                    return BadRequest(new Contracts.Responses.Common.ErrorResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
            }
            else
            {
                return BadRequest(new Contracts.Responses.Common.ErrorResponse
                {
                    Status = "error",
                    Message = "Email or username is required"
                });
            }
        }


        [HttpPost("validate-reset-code")]
        [ProducesResponseType(typeof(Contracts.Responses.Password.ValidateResetCodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateResetCode([FromBody] Contracts.Requests.Password.ValidateResetCodeRequest request)
        {
            if (request.Code.Length != 32)
            {
                return BadRequest(new Contracts.Responses.Common.ErrorResponse
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
                    return Ok(new Contracts.Responses.Password.ValidateResetCodeResponse
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
                        return StatusCode(StatusCodes.Status500InternalServerError, new Contracts.Responses.Common.ErrorResponse
                        {
                            Status = response.status,
                            Message = response.message ?? "Internal server error, please try again later, if issue persists contact support."
                        });
                    }
                    else
                    {
                        return BadRequest(new Contracts.Responses.Common.ErrorResponse
                        {
                            Status = response.status,
                            Message = response.message
                        });
                    }
                }
            }
        }

        [HttpPost("reset")]
        [ProducesResponseType(typeof(Contracts.Responses.Password.ResetPasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] Contracts.Requests.Password.ResetPasswordRequest request)
        {
            var response = await _passwordService.ChangePassword(request.Code, request.NewPassword);
            if (response.isSuccess)
            {
                return Ok(new Contracts.Responses.Password.ResetPasswordResponse
                {
                    Status = response.status,
                    Message = response.message
                });
            }
            else
            {
                if (response.status == "INTERNAL_ERROR")
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Contracts.Responses.Common.ErrorResponse
                    {
                        Status = response.status,
                        Message = response.message ?? "Internal server error, please try again later, if issue persists contact support."
                    });
                }
                else
                {
                    return BadRequest(new Contracts.Responses.Common.ErrorResponse
                    {
                        Status = response.status,
                        Message = response.message
                    });
                }
            }
        }
    }
}
