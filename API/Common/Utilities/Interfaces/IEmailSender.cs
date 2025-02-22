
namespace API.Common.Utilities.Interfaces;

public interface IEmailSender
{
    Task<bool> SendOtpEmailAsync(string toEmail, string otp);
    Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash);
}