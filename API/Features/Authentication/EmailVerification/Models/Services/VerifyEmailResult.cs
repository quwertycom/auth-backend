using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.EmailVerification.Models.Services;

public record VerifyEmailResult : ServiceResult {
  public string? RequestId { get; set; }
}
