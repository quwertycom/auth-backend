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

    public async Task<(bool isSuccess, string status, string message)> ChangePassword(string code, string Password)
    {
        try
        {
            var (codeHash, _) = Hasher.Hash(code, "");
            var request = await _UserRepository.GetResetPasswordRequestByCodeHash(codeHash);
            if (request == null)
            {
                return (false, "INVALID_CODE", "Invalid reset code");
            }
            else if (request.IsUsed)
            {
                return (false, "USED_CODE", "Reset code has already been used");
            }
            else if (request.ExpiredAt <= DateTime.UtcNow)
            {
                return (false, "EXPIRED_CODE", "Reset code has expired");
            }
            else
            {
                if (Password.Length < 8)
                {
                    return (false, "PASSWORD_TOO_SHORT", "Password must be at least 8 characters long.");
                }
                var (newHash, newSalt) = Hasher.Hash(Password);
                System.Console.WriteLine("New Hash: " + newHash);
                await _UserRepository.UpdateUserPassword(request.User, newHash, newSalt);
                request.IsUsed = true;
                await _Context.SaveChangesAsync();
                Console.WriteLine("Password changed successfully");
                return (true, "SUCCESS", "Password changed successfully");
            }
        }
        catch (Exception ex)
        {
            return (false, "INTERNAL_ERROR", ex.Message ?? "Internal server error, please try again later, if issue persists contact support.");
        }
    }

    public async Task<(bool isSuccess, string status, string message)> RequestResetViaEmail(string email)
    {
        try
        {
            var Email = await _UserRepository.GetEmailModelByEmail(email);
            if (Email != null)
            {
                var User = await _UserRepository.GetUserById(Email.UserId);
                if (User != null && User.State == UserState.Active)
                {
                    var code = RandomGenerator.GenerateAlphanumericCode(32);
                    var (codeHash, _) = Hasher.Hash(code, "");
                    var request = await _UserRepository.CreateResetPasswordRequest(User, Email, codeHash);
                    if (request != null)
                    {
                        var EmailSentSuccessfully = await EmailSender.SendResetPasswordEmailAsync(Email.Email, code);
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
            var User = await _UserRepository.GetUserByUsername(username);
            if (User != null)
            {
                var Email = await _UserRepository.GetEmailModelByUserId(User.Id);
                if (Email != null)
                {
                    var code = RandomGenerator.GenerateAlphanumericCode(32);
                    var (codeHash, _) = Hasher.Hash(code, "");
                    var request = await _UserRepository.CreateResetPasswordRequest(User, Email, codeHash);
                    if (request != null)
                    {
                        var EmailSentSuccessfully = await EmailSender.SendResetPasswordEmailAsync(Email.Email, code);
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

    public async Task<(bool isSuccess, string status, string message, bool isValid)> ValidateResetCode(string code)
    {
        try
        {
            var (codeHash, _) = Hasher.Hash(code, "");
            var request = await _UserRepository.GetResetPasswordRequestByCodeHash(codeHash);

            if (request == null)
                return (false, "INVALID_CODE", "Invalid reset code", false);
            else if (request.ExpiredAt < DateTime.UtcNow)
                return (false, "EXPIRED_CODE", "Reset code has expired", false);
            else if (request.IsUsed)
                return (false, "USED_CODE", "Reset code has already been used", false);
            else
                return (true, "SUCCESS", "Reset code is valid", true);
        }
        catch (Exception ex)
        {
            return (false, "INTERNAL_ERROR", ex.Message ?? "Internal server error, please try again later, if issue persists contact support.", false);
        }
    }
}