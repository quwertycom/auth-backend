using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Web.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;

    public ApiController(ILogger<ApiController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        try
        {
            _logger.LogInformation("Executing GET request to /api endpoint");
            return Ok(new
            {
                message = "API is running",
                timestamp = DateTime.UtcNow,
                status = "healthy"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing GET request to /api endpoint");
            return StatusCode(500, new ProblemDetails
            {
                Status = 500,
                Title = "An error occurred while processing your request",
                Detail = ex.Message
            });
        }
    }

    [HttpGet("test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Test()
    {
        try
        {
            _logger.LogInformation("Executing GET request to /api/test endpoint");
            return Ok(new
            {
                message = "Test endpoint working",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing GET request to /api/test endpoint");
            return StatusCode(500, new ProblemDetails
            {
                Status = 500,
                Title = "An error occurred while processing your request",
                Detail = ex.Message
            });
        }
    }
}