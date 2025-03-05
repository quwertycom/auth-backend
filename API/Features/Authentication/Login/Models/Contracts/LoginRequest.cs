
namespace API.Features.Authentication.Login.Models.Contracts;

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}