using API.Features.Authentication.EmailVerification.Models.Services;

namespace API.Features.Authentication.EmailVerification.Interfaces;

public interface IEmailVerificationService
{
    Task<RequestEmailVerificationResult> RequestEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken);
    Task<GetRequestStatusResult> GetRequestStatusAsync(string requestId, string email, CancellationToken cancellationToken);
    Task<VerifyEmailResult> VerifyEmailAsync(string requestId, string code, CancellationToken cancellationToken);
    Task<RequestNewCodeResult> RequestNewCodeAsync(string email, CancellationToken cancellationToken);
}
