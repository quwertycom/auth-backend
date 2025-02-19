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
    [ProducesResponseType(typeof(Contracts.Responses.Token.ValidateTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateTokenAsync([FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return BadRequest("Invalid authorization header");
        
        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (token == null || token == "")
        {
            return BadRequest(new Contracts.Responses.Common.ErrorResponse
            {
                Status = "error",
                Message = "Token is required"
            });
        }
        else
        {
            var result = await _tokenService.ValidateAsync(token);
            if (result.isSuccess) {
                if (result.isValid) {
                    return Ok(new Contracts.Responses.Token.ValidateTokenResponse
                    {
                        Status = result.status ?? "SUCCESS",
                        Message = result.message ?? "Token is valid",
                        IsValid = result.isValid
                    });
                }
                else
                {
                    return Ok(new Contracts.Responses.Token.ValidateTokenResponse
                    {
                        Status = result.status ?? "SUCCESS",
                        Message = result.message ?? "Token is not valid",
                        IsValid = false
                    });
                }
            }
            else
            {
                if (result.status == "INTERNAL_ERROR")
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Contracts.Responses.Common.ErrorResponse
                    {
                        Status = result.status ?? "INTERNAL_ERROR",
                        Message = result.message ?? "Internal server error, please try again later, if issue persists contact support."
                    });
                }
                else
                {
                    return BadRequest(new Contracts.Responses.Common.ErrorResponse
                    {
                        Status = result.status ?? "ERROR",
                        Message = result.message ?? "Failed to validate token."
                    });
                }
            }
        }
    }
}