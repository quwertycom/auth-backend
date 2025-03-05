using API.Shared.Contracts.Responses.Common;

namespace API.Features.Authentication.Login.Models.Contracts;

public record LoginResponse : ResponseBase
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}