using Auth.Models.RequestModels;
using Auth.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    [HttpPost("auth/login")]
    public async Task<ActionResult<UserLoginResponseModel>> Login([FromBody] UserLoginRequestModel request)
    {
        return Ok(new UserLoginResponseModel
        {
            Status = "Success",
        });
    }

    [HttpPost("auth/register")]
    public async Task<ActionResult<UserRegisterResponseModel>> Register([FromBody] UserRegisterRequestModel request)
    {
        return Ok(new UserRegisterResponseModel
        {
            Status = "Success",
        });
    }
}