
namespace API.Features.Session.Revoke.Models.Contracts;

public record RevokeSessionRequest
{
  public required string SessionId { get; set; }
}