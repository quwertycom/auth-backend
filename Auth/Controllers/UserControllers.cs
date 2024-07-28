using Auth.Models.RequestModels;
using Auth.Models.ResponseModels;
using Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

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
        var validators = new Dictionary<Func<UserRegisterRequestModel, bool>, string>
        {
            { r => string.IsNullOrEmpty(r.Email), "EMPTY_EMAIL" },
            { r => !r.Email.Contains("@") || !r.Email.Contains("."), "INVALID_EMAIL" },
            { r => string.IsNullOrEmpty(r.FirstName), "EMPTY_FIRST_NAME" },
            { r => r.FirstName.Length < 3 || r.FirstName.Length > 32, "INVALID_FIRST_NAME" },
            { r => string.IsNullOrEmpty(r.LastName), "EMPTY_LAST_NAME" },
            { r => r.LastName.Length < 3 || r.LastName.Length > 32, "INVALID_LAST_NAME" },
            { r => string.IsNullOrEmpty(r.Password), "EMPTY_PASSWORD" },
            { r => r.Password.Length < 8 || r.Password.Length > 32 || r.Password.Any(char.IsDigit) || r.Password.Any(char.IsLetter), "INVALID_PASSWORD" },
            { r => r.Phone != null && (r.Phone.Length < 5 || r.Phone.Length > 15 || r.Phone.All(char.IsDigit)), "INVALID_PHONE" },
            { r => string.IsNullOrEmpty(r.Username), "EMPTY_USERNAME" },
            { r => r.Username.Length < 3 || r.Username.Length > 32 || r.Username.Any(char.IsWhiteSpace), "INVALID_USERNAME" },
            { r => string.IsNullOrEmpty(r.BirthDate), "EMPTY_BIRTH_DATE" },
            { r => !DateTime.TryParse(r.BirthDate, out var birthDate) || birthDate > DateTime.UtcNow.AddYears(-17), "INVALID_BIRTH_DATE" },
            { r => string.IsNullOrEmpty(r.Gender), "EMPTY_GENDER" },
        };

        foreach (var validator in validators)
        {
            if (validator.Key(request))
            {
                return BadRequest(new UserRegisterResponseModel
                {
                    Status = validator.Value
                });
            }
        }

        var result = await _userService.Register(request);

        if (result.isSuccess)
        {
            return Ok(new UserRegisterResponseModel
            {
                Status = "Success"
            });
        }
        else
        {
            return BadRequest(new UserRegisterResponseModel
            {
                Status = result.status
            });
        }
    }
}