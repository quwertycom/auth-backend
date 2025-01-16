using System.Text.RegularExpressions;
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
    Task<(bool isSuccess, string status, string message, long? verificationSessionID)> VerifyEmailAsync(VerifyEmailRequest request);
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
        try
        {
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            var usernameRegex = new Regex(@"^[a-zA-Z0-9_-]{3,50}$");
            var phoneRegex = new Regex(@"^\+?\d{1,4}[-.\s]?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,4}$");

            if (!emailRegex.IsMatch(request.Email))
            {
                return (false, "INVALID_EMAIL", "Invalid email format.", null);
            }

            if (!usernameRegex.IsMatch(request.Username))
            {
                return (false, "INVALID_USERNAME", "Invalid username format.", null);
            }

            if (request.PhoneNumber != null && !phoneRegex.IsMatch(request.PhoneNumber))
            {
                return (false, "INVALID_PHONE_NUMBER", "Invalid phone number format.", null);
            }

            var usernameExists = await _dbContext.Users.AnyAsync(u => u.Username == request.Username);
            if (usernameExists)
            {
                return (false, "USERNAME_TAKEN", "Username already exists, please try a different username.", null);
            }

            var emailExists = await _dbContext.UserEmails.AnyAsync(ue => ue.Email == request.Email && ue.State == EmailState.Verified);
            if (emailExists)
            {
                return (false, "EMAIL_TAKEN", "Email already exists, please try a different email.", null);
            }

            var phoneNumberExists = request.PhoneNumber != null && await _dbContext.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (phoneNumberExists)
            {
                return (false, "PHONE_NUMBER_TAKEN", "Phone number already exists, please try a different phone number.", null);
            }

            if (request.Password.Length < 8)
            {
                return (false, "PASSWORD_TOO_SHORT", "Password must be at least 8 characters long.", null);
            }

            var hashedPassword = PasswordHasher.Hash(request.Password);

            var user = new User
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = hashedPassword.hash,
                PasswordSalt = hashedPassword.salt,
            };

            var existingEmail = await _dbContext.UserEmails.FirstOrDefaultAsync(ue => ue.Email == request.Email);
            if (existingEmail != null)
            {
                _dbContext.UserEmails.Remove(existingEmail);
            }

            var email = new UserEmail
            {
                Email = request.Email,
                State = EmailState.Unverified,
                IsPrimary = true,
                User = user,
            };

            var otp = OTPGenerator.GenerateOTP();

            var otpSession = new VerificationSession
            {
                Email = email,
                User = user,
                Code = otp,
                IsUsed = false,
            };

            _dbContext.Users.Add(user);
            _dbContext.UserEmails.Add(email);
            _dbContext.VerificationSessions.Add(otpSession);
            await _dbContext.SaveChangesAsync();

            await EmailSender.SendOtpEmailAsync(email.Email, otp);

            return (true, "OTP_SENT", "8 Digits code has been sent to your email. Please verify your email and login.", otpSession.Id);
        }
        catch
        {
            return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null);
        }
    }

    public async Task<(bool isSuccess, string status, string message, long? verificationSessionID)> VerifyEmailAsync(VerifyEmailRequest request)
    {
        try
        {
            var verificationSession = await _dbContext.VerificationSessions
                .Include(vs => vs.User)
                .Include(vs => vs.Email)
                .FirstOrDefaultAsync(vs => vs.Id == request.VerificationSessionID);

            if (verificationSession == null)
            {
                return (false, "NOT_FOUND", "Verification session not found.", null);
            }

            if (verificationSession.Code != request.OTP)
            {
                return (false, "INVALID_OTP", "Invalid OTP.", null);
            }

            if (verificationSession.IsUsed)
            {
                return (false, "ALREADY_USED", "OTP already used.", null);
            }

            if (verificationSession.CreatedAt.AddMinutes(verificationSession.ExpiryMinutes) < DateTime.UtcNow)
            {
                return (false, "EXPIRED", "OTP expired.", null);
            }

            var email = await _dbContext.UserEmails
                .Include(ue => ue.User)
                .FirstOrDefaultAsync(ue => ue.Email == request.Email);

            if (email == null)
            {
                return (false, "NOT_FOUND", "Email not found.", null);
            }

            if (verificationSession.EmailId != email.UserEmailId)
            {
                return (false, "INVALID_SESSION", "Invalid verification session for this email.", null);
            }

            verificationSession.IsUsed = true;
            email.State = EmailState.Verified;
            email.IsPrimary = true;

            await _dbContext.SaveChangesAsync();

            return (true, "SUCCESS", "Email verified successfully.", verificationSession.Id);
        }
        catch
        {
            return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null);
        }
    }
}