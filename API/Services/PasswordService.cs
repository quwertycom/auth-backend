using System.Text.Json;
using API.Common.Enums;
using API.Common.Helpers;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
namespace API.Services;
public class PasswordService : IPasswordService
{
    private readonly AuthDbContext _Context;
    private readonly IUserRepository _UserRepository;
    public PasswordService(AuthDbContext context, IUserRepository userRepository)
    {
        _Context = context;
        _UserRepository = userRepository;
    }

    public async Task<(bool isSuccess, string status, string message)> ChangePassword(long UsertId, string Password, string otp)
    {
        var User = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UsertId);
        if (User != null && await CheckIsOTPValid(UsertId, otp))
        {
            var hashedPassword = Hasher.Hash(Password);
            User.PasswordHash = hashedPassword.hash;
            User.PasswordSalt = hashedPassword.salt;
            await _Context.SaveChangesAsync();
            return (true, "success", "Password changed successfully");
        }
        return (false, "error", "Invalid OTP");
    }

    public async Task<(bool isSuccess, string status, string message)> RequestResetViaEmail(string email)
    {
        try
        {
            var Email = await _Context.UserEmails
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Email == email && x.State == EmailState.Verified);
            if (Email != null)
            {
                var User = Email.User;
                if (User != null && User.State == UserState.Active)
                {
                    var requestResponse = await _UserRepository.SendResetPasswordRequest(User.Id);
                    if (requestResponse.request != null)
                    {
                        var EmailSentSuccessfully = await EmailSender.SendResetPasswordEmailAsync(Email.Email, requestResponse.code);
                        if (EmailSentSuccessfully)
                        {
                            return (true, "SUCCESS", "Reset password request sent successfully");
                        }
                        else
                        {
                            return (false, "SENT_FAILED", "Failed to send reset password request");
                        }
                    }
                    else
                    {
                        return (false, "SENT_FAILED", "Failed to send reset password request");
                    }
                }
                else
                {
                    return (false, "USER_NOT_FOUND", "User not found");
                }
            }
            else
            {
                return (false, "EMAIL_NOT_FOUND", "Email not found");
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("NOT_FOUND"))
                return (false, "USER_NOT_FOUND", "User not found");
            else if (ex.Message.Contains("EMAIL_NOT_FOUND"))
                return (false, "EMAIL_NOT_FOUND", "Email not found");
            else
                return (false, "INTERNAL_ERROR", ex.Message ?? "Internal server error, please try again later, if issue persists contact support.");
        }
    }

    public async Task<(bool isSuccess, string status, string message)> RequestResetViaUsername(string username)
    {
        try
        {
            var User = await _Context.Users.FirstOrDefaultAsync(x => x.Username == username && x.State == UserState.Active);
            if (User != null)
            {
                var Email = await _Context.UserEmails
                    .FirstOrDefaultAsync(x => x.UserId == User.Id && x.Type == EmailType.Primary && x.State == EmailState.Verified);
                if (Email != null)
                {
                    var requestResponse = await _UserRepository.SendResetPasswordRequest(User.Id);
                    if (requestResponse.request != null)
                    {
                        var EmailSentSuccessfully = await EmailSender.SendResetPasswordEmailAsync(Email.Email, requestResponse.code);
                        if (EmailSentSuccessfully)
                        {
                            return (true, "SUCCESS", "Reset password request sent successfully");
                        }
                        else
                        {
                            return (false, "SENT_FAILED", "Failed to send reset password request");
                        }
                    }
                    else
                    {
                        return (false, "SENT_FAILED", "Failed to send reset password request");
                    }
                }
                else
                {
                    return (false, "EMAIL_NOT_FOUND", "Primary email not found");
                }
            }
            else
            {
                return (false, "USER_NOT_FOUND", "User not found");
            }
        }
        catch (Exception ex)
        {
            return (false, "INTERNAL_ERROR", ex.Message ?? "Internal server error, please try again later, if issue persists contact support.");
        }
    }
    private async Task<bool> CheckIsOTPValid(long UserId, string OTP)
    {
        var UserOTP = await _Context.VerificationSessions.FirstOrDefaultAsync(x => x.UserId == UserId && x.CreatedAt.AddMinutes(x.ExpiryMinutes) > DateTime.Now);
        if (UserOTP != null)
        {
            if (UserOTP.Code.Equals(OTP))
            {
                return true;
            }
        }
        return false;
    }
}