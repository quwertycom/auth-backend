using API.Services.Interfaces;
using API.Contracts.Requests.Auth;
using System.Text.RegularExpressions;

namespace API.IntegrationTests.Mocks;

public class MockAuthService : IAuthService
{
    public Task<(bool, string, string, long?)> RegisterUserAsync(RegisterRequest request)
    {
        var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$");
        return emailRegex.IsMatch(request.Email) 
            ? Task.FromResult((true, "SUCCESS", "Mock message", (long?)1L))
            : Task.FromResult((false, "INVALID_EMAIL", "Invalid email format", (long?)null));
    }

    public Task<(bool, string, string, long?)> VerifyEmailAsync(VerifyEmailRequest request)
        => Task.FromResult<(bool, string, string, long?)>((true, "SUCCESS", "Mock message", null));

    public Task<(bool, string, string, string?, string?)> LoginAsync(LoginRequest request)
        => Task.FromResult<(bool, string, string, string?, string?)>(
            (true, "SUCCESS", "Mock message", "mock-token", "mock-refresh"));
} 