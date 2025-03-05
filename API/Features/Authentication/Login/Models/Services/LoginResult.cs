
using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.Login.Models.Services;

public record LoginResult : ServiceResult
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}