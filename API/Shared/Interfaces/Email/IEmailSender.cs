namespace API.Shared.Interfaces.Email;

/// <summary>
/// Interface for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an OTP email to the specified email address.
    /// </summary>
    /// <param name="toEmail">The recipient's email address</param>
    /// <param name="otp">The one-time password code</param>
    /// <param name="language">The language code (e.g., 'en', 'es'). Defaults to 'en'</param>
    Task<bool> SendOtpEmailAsync(string toEmail, string otp, string language = "en");

    /// <summary>
    /// Sends a reset password email to the specified email address.
    /// </summary>
    /// <param name="toEmail">The recipient's email address</param>
    /// <param name="codeHash">The reset password code hash</param>
    /// <param name="language">The language code (e.g., 'en', 'es'). Defaults to 'en'</param>
    Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash, string language = "en");
}