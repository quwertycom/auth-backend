using API.Core.Contracts.Responses;

namespace API.Core.Contracts.Responses.Auth;

public class LoginResponse : ResponseBase
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}