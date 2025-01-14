using API.Common.Helpers;
using API.Data;

namespace API.Service;

public interface IAuthService
{
    Task<(string status, string message, string accessToken, string refreshToken)> RegisterUserAsync(string email, string password);
}

public class AuthService : IAuthService
{
    private readonly AuthDbContext _dbContext;

    public AuthService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(string status, string message, string accessToken, string refreshToken)> RegisterUserAsync(string email, string password)
    {
        return ("", "", "", "");
    }
}