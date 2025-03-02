using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.EmailVerification.Models.Services;

public record GetVerificationSessionStatusResult : ServiceResult {
  public required bool IsValid { get; set; }
}