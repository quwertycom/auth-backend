using API.Common.Enums;
using API.Common.Helpers;
using API.Contracts.Requests.Auth;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service;

public interface IAuthService
{
    Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(RegisterRequest request);
}

public class AuthService : IAuthService
{
    private readonly AuthDbContext _dbContext;

    public AuthService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(RegisterRequest request)
    {
        try {
            var userExists = await _dbContext.Users.AnyAsync(u => u.Emails.Any(ue => ue.Email == request.Email && ue.State == EmailState.Verified) || u.Username == request.Username || u.PhoneNumber == request.PhoneNumber);
            if (userExists) {
                return (false, "EMAIL_TAKEN", "Email already exists, please try a different email.", null);
            }

            if (request.Password.Length < 8) {
                return (false, "PASSWORD_TOO_SHORT", "Password must be at least 8 characters long.", null);
            }

            var hashedPassword = PasswordHasher.Hash(request.Password);

            var user = new User {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = hashedPassword.hash,
                PasswordSalt = hashedPassword.salt,
            };

            var existingEmail = await _dbContext.UserEmails.FirstOrDefaultAsync(ue => ue.Email == request.Email);
            if (existingEmail != null) {
                _dbContext.UserEmails.Remove(existingEmail);
            }

            var email = new UserEmail {
                Email = request.Email,
                State = EmailState.Unverified,
                IsPrimary = true,
                User = user,
            };

            var otp = OTPGenerator.GenerateOTP();

            var otpSession = new VerificationSession {
                Email = email,
                Code = otp,
                IsUsed = false,
            };

            _dbContext.Users.Add(user);
            _dbContext.UserEmails.Add(email);
            _dbContext.VerificationSessions.Add(otpSession);
            await _dbContext.SaveChangesAsync();

            return (true, "SUCCESS", "User registered successfully", otpSession.Id);
        } catch {
            return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null);
        }
    }
}