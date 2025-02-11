using API.Models;
using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Contracts.Requests.Password;
using API.Contracts.Responses.Password;
using API.Contracts.Responses.Common;

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
                    Message = "Email is required"
                });
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword()
        {
            return Ok();
        }
    }
}
