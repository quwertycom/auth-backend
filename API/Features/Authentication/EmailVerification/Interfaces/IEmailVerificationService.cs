using API.Features.Authentication.EmailVerification.Models.Services;

namespace API.Features.Authentication.EmailVerification.Interfaces;

public interface IEmailVerificationService {
  Task<RequestEmailVerificationResult> RequestEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken);
  Task<GetSessionStatusResult> GetSessionStatusAsync(long sessionId, string email, CancellationToken cancellationToken);
  Task<VerifyEmailResult> VerifyEmailAsync(long sessionId, string code, CancellationToken cancellationToken);
}
