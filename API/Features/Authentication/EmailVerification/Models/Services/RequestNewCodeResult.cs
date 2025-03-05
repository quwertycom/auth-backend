
using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.EmailVerification.Models.Services;

public record RequestNewCodeResult : ServiceResult {
  public string? NewRequestId { get; set; }
}