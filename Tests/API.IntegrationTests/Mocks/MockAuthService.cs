using API.Services.Interfaces;
using API.Contracts.Requests.Auth;

namespace API.IntegrationTests.Mocks;

public class MockAuthService : IAuthService
{
    public Task<(bool, string, string, long?)> RegisterUserAsync(RegisterRequest request)
        => Task.FromResult((true, "SUCCESS", "Mock message", (long?)1L));

    public Task<(bool, string, string, long?)> VerifyEmailAsync(VerifyEmailRequest request)
        => Task.FromResult<(bool, string, string, long?)>((true, "SUCCESS", "Mock message", null));

    public Task<(bool, string, string, string?, string?)> LoginAsync(LoginRequest request)
        => Task.FromResult<(bool, string, string, string?, string?)>(
            (true, "SUCCESS", "Mock message", "mock-token", "mock-refresh"));
} 