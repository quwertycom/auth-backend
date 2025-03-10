using API.Features.User.Password.Reset.Models.Services;

namespace API.Features.User.Password.Reset.Interfaces;

public interface IResetPasswordService
{
    Task<RequestPasswordResetResult> RequestPasswordResetViaEmailAsync(string email, CancellationToken cancellationToken);
    Task<CheckRequestStatusResult> CheckRequestStatusAsync(string code, CancellationToken cancellationToken);
}
