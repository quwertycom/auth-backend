using API.Shared.Models.Features.Services;

namespace API.Features.Session.Refresh.Models.Services;

public record RefreshSessionResult : ServiceResult {
  public string? AccessToken { get; set; }
  public string? RefreshToken { get; set; }
}
