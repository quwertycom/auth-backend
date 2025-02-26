using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/session")]
[Produces("application/json")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("revoke")]
    [ProducesResponseType(typeof(Contracts.Responses.Common.SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Contracts.Responses.Common.ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeSessionByToken([FromHeader(Name = "Authorization")] string authHeader)
    {
        var token = authHeader.Split(" ")[1];
        var result = await _sessionService.RevokeSessionByToken(token);
        if (result.isSuccess)
        {
            return Ok(new Contracts.Responses.Common.SuccessResponse { Status = "SUCCESS", Message = result.message ?? "Session revoked successfully." });
        }
        else if (result.status == "NOT_FOUND")
        {
            return NotFound(new Contracts.Responses.Common.ErrorResponse { Status = "NOT_FOUND", Message = result.message ?? "Session not found." });
        }
        else if (result.status == "ALREADY_REVOKED")
        {
            return BadRequest(new Contracts.Responses.Common.ErrorResponse { Status = "ALREADY_REVOKED", Message = result.message ?? "Session already revoked." });
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Contracts.Responses.Common.ErrorResponse { Status = "INTERNAL_ERROR", Message = result.message ?? "Internal server error, please try again later, if issue persists contact support." });
        }
    }
}
