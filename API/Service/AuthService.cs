using API.Common.Helpers;
using API.Data;

namespace API.Service;

public interface IAuthService
{
    Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(string email, string password);
}

public class AuthService : IAuthService
{
    private readonly AuthDbContext _dbContext;

    public AuthService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(string email, string password)
    {
        await Task.Delay(1000);
        return (true, "success", "User registered successfully", 1);
    }
}