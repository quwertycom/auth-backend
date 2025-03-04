
namespace API.Features.Authentication.EmailVerification.Models.Contracts;

public record VerifyEmailRequest {
  public required string RequestId { get; set; }
  public required string Code { get; set; }
}