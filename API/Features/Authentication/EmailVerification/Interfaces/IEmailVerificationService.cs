using API.Features.Authentication.EmailVerification.Models.Services;

namespace API.Features.Authentication.EmailVerification.Interfaces;

public interface IEmailVerificationService {
  Task<RequestEmailVerificationResult> RequestEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken);
  Task<GetVerificationSessionStatusResult> GetVerificationSessionStatusAsync(string emailVerificationSessionId, CancellationToken cancellationToken);
  Task<VerifyEmailResult> VerifyEmailAsync(string emailVerificationSessionId, string code, CancellationToken cancellationToken);
}
