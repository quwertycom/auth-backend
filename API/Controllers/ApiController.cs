using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Hello World");
}