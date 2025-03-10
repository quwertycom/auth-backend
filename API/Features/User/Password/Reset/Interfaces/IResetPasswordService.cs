using API.Features.User.Password.Reset.Models.Services;

namespace API.Features.User.Password.Reset.Interfaces;

public interface IResetPasswordService
{
    Task<RequestPasswordResetResult> RequestPasswordResetViaEmailAsync(string email, CancellationToken cancellationToken);
    Task<RequestPasswordResetResult> RequestPasswordResetViaUsernameAsync(string username, CancellationToken cancellationToken);
    Task<CheckRequestStatusResult> CheckRequestStatusAsync(string code, CancellationToken cancellationToken);
    Task<ResetPasswordResult> ResetPasswordAsync(string code, string newPassword, CancellationToken cancellationToken);
}
