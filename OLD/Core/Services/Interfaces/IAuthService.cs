using API.Core.Contracts.Requests.Auth;

namespace API.Core.Services.Interfaces;

public interface IAuthService
{
    Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(RegisterRequest request);
    Task<(bool isSuccess, string status, string message, long? verificationSessionID)> VerifyEmailAsync(VerifyEmailRequest request);
    Task<(bool isSuccess, string status, string message, string? accessToken, string? refreshToken)> LoginAsync(LoginRequest request);
}