
namespace API.Shared.Utilities.Interfaces;

/// <summary>
/// Interface for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an OTP email to the specified email address.
    /// </summary>
    Task<bool> SendOtpEmailAsync(string toEmail, string otp);

    /// <summary>
    /// Sends a reset password email to the specified email address.
    /// </summary>
    Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash);
}