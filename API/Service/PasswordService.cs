using System.Text.Json;
using API.Common.Enums;
using API.Common.Helpers;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using System.Text.Json.Serialization;

namespace API.Service;
public interface IPasswordService
{
    public Task<(bool isSuccess, string status, string message)> ChangePassword(long UsertId, string Password, string otp);
    public Task<(bool isSuccess, string status, string message)> RequestResetViaEmail(string Email);
    public Task<(bool isSuccess, string status, string message)> RequestResetViaUsername(string Username);
}
public class PasswordService : IPasswordService
{
    private readonly AuthDbContext _Context;
    private readonly IUserInfoRepository _UserInfoRepository;
    public PasswordService(AuthDbContext context, IUserInfoRepository userInfoRepository)
    {
        _Context = context;
        _UserInfoRepository = userInfoRepository;
    }

    public async Task<(bool isSuccess, string status, string message)> ChangePassword(long UsertId, string Password, string otp)
    {
        var User = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UsertId);
        if (User != null && await CheckIsOTPValid(UsertId, otp))
        {
            var hashedPassword = PasswordHasher.Hash(Password);
            User.PasswordHash = hashedPassword.hash;
            User.PasswordSalt = hashedPassword.salt;
            await _Context.SaveChangesAsync();
            return (true, "success", "Password changed successfully");
        }
        return (false, "error", "Invalid OTP");
    }
    // public async Task SendOTP(long UserId)
    // {
    // 	var Email = await _Context.UserEmails.FirstOrDefaultAsync(x => x.UserId == UserId);
    // 	var User = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
    // 	if (User != null && Email != null)
    // 	{
    // 		var otp = OTPGenerator.GenerateOTP();
    // 		var otpSession = new VerificationSession
    // 		{
    // 			Email = Email,
    // 			User = User,
    // 			Code = otp,
    // 			IsUsed = false,
    // 		};
    // 		_Context.VerificationSessions.Add(otpSession);
    // 		if (await _Context.SaveChangesAsync() > 0)
    // 		{
    // 			await EmailSender.SendOtpEmailAsync(Email.Email, otp);
    // 		}
    // 	}
    // }

    public async Task<(bool isSuccess, string status, string message)> RequestResetViaEmail(string email)
    {
        try
        {
            var Email = await _Context.UserEmails
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Email == email);
            Console.WriteLine("Email: " + Email?.Email ?? "Not found");
            Console.WriteLine(JsonSerializer.Serialize(Email, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }));
            if (Email != null)
            {
                var User = Email.User;
                Console.WriteLine("User: " + User?.Id ?? "Not found");
                if (User != null)
                {
                    var ResetRequest = await _UserInfoRepository.SendResetPasswordRequest(User.Id);
                    Console.WriteLine("ResetRequest: " + ResetRequest?.OTP ?? "Not found");
                    if (ResetRequest != null)
                    {
                        var EmailSentSuccessfully = await EmailSender.SendOtpEmailAsync(Email.Email, ResetRequest.OTP);
                        Console.WriteLine("EmailSentSuccessfully: " + EmailSentSuccessfully);
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
            return (false, "INTERNAL_ERROR", ex.Message ?? "Internal server error, please try again later, if issue persists contact support.");
        }
    }

    public async Task<(bool isSuccess, string status, string message)> RequestResetViaUsername(string username)
    {
        try
        {
            var User = await _Context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (User != null)
            {
                var primaryEmail = await _Context.UserEmails.FirstOrDefaultAsync(x => x.UserId == User.Id && x.Type == EmailType.Primary && x.State == EmailState.Verified);
                if (primaryEmail != null)
                {
                    var ResetRequest = await _UserInfoRepository.SendResetPasswordRequest(User.Id);
                    if (ResetRequest != null)
                    {
                        var EmailSentSuccessfully = await EmailSender.SendOtpEmailAsync(primaryEmail.Email, ResetRequest.OTP);
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