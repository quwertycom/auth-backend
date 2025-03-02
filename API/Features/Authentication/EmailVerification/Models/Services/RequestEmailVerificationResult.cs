
using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.EmailVerification.Models.Services;

public record RequestEmailVerificationResult : ServiceResult {
  public string? EmailVerificationSessionId { get; set; }
}
