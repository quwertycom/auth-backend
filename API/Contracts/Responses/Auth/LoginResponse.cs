using API.Contracts.Responses;

namespace API.Contracts.Responses.Auth;

public class LoginResponse : ResponseBase
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}