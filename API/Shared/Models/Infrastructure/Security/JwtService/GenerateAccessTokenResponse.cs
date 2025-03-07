
namespace API.Shared.Models.Infrastructure.Security.JwtService;

public record GenerateAccessTokenResponse
{
  public required bool IsSuccess { get; set; }
  public required string Status { get; set; }
  public string? Message { get; set; }
  public string? AccessToken { get; set; }
}