using API.Shared.Contracts.Responses.Common;

namespace API.Features.Session.Refresh.Models.Contracts;

public record RefreshSessionResponse : ResponseBase
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}