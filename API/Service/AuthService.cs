using API.Common.Enums;
using API.Common.Helpers;
using API.Data;
using Microsoft.EntityFrameworkCore;

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
        try {
            var emailExists = await _dbContext.UserEmails.AnyAsync(ue => ue.Email == email && ue.State == EmailState.Verified);
            if (emailExists) {
                return (false, "EMAIL_TAKEN", "Email already exists, please try a different email.", null);
            }
            
            if (password.Length < 8) {
                return (false, "PASSWORD_TOO_SHORT", "Password must be at least 8 characters long.", null);
            }
            
            return (true, "SUCCESS", "User registered successfully", 1);
        } catch {
            return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null);
        }
    }
}