using API.Contracts.Responses;

namespace qAuth.API.Contracts.Responses.Auth;

public class LoginResponse : ResponseBase
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}