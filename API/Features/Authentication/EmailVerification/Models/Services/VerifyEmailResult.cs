using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.EmailVerification.Models.Services;

public record VerifyEmailResult : ServiceResult {
  public required string VerificationSessionId { get; set; }
  public required string Code { get; set; }
}
