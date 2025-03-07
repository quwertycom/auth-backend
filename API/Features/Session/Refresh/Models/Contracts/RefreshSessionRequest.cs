
namespace API.Features.Session.Refresh.Models.Contracts;

public record RefreshSessionRequest
{
  public required string Token { get; set; }
}