using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.EmailVerification.Models.Services;

public record GetVerificationSessionStatusResult : ServiceResult {
  public bool? IsValid { get; set; }
}