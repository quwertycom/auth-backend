using API.Features.Authentication.Password.Reset.Models.Services;
namespace API.Features.Authentication.Password.Reset.Interfaces;

public interface IResetPasswordService
{
    Task<RequestPasswordResetResult> RequestPasswordResetViaEmailAsync(string email, CancellationToken cancellationToken);
}
