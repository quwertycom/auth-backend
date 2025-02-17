using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using API.Configuration;

namespace API.Common.Helpers;

public static class EmailSender
{
    private static SmtpClient? _smtpClient;
    private static string? _fromEmail;
    private static bool _isInitialized = false;
    private static string? _frontendUrl;
    public static void Initialize(IConfiguration configuration)
    {
        if (_isInitialized) return;

        var settings = configuration.GetSection("Email").Get<EmailSettings>()
            ?? throw new InvalidOperationException("Email settings are not configured");

        _smtpClient = new SmtpClient
        {
            Host = settings.Host,
            Port = settings.Port,
            EnableSsl = settings.EnableSsl,
            Credentials = new NetworkCredential(settings.Username, settings.Password),
            Timeout = settings.Timeout,
            UseDefaultCredentials = settings.UseDefaultCredentials
        };

        _fromEmail = settings.FromEmail;
        _isInitialized = true;
        _frontendUrl = configuration.GetSection("Frontend").GetSection("BaseUrl").Value ?? "http://localhost:3000";
    }

    public static async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("EmailSender is not initialized. Call Initialize() first.");
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail ?? throw new InvalidOperationException("FromEmail is not configured")),
            Subject = "Your OTP Code",
            Body = $"Your OTP code is: {otp}",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);

        try
        {
            if (_smtpClient is null) throw new InvalidOperationException("SMTP client is not initialized.");
            await _smtpClient.SendMailAsync(mailMessage).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("ERROR: Failed to send OTP email, " + ex.Message);
        }
    }

    public static async Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("EmailSender is not initialized. Call Initialize() first.");
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail ?? throw new InvalidOperationException("FromEmail is not configured")),
            Subject = "Reset Password",
            Body = $"Your reset password link is: {_frontendUrl}/app/auth/reset-password?code={codeHash}",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);

        try
        {
            if (_smtpClient is null) throw new InvalidOperationException("SMTP client is not initialized.");
            await _smtpClient.SendMailAsync(mailMessage).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("ERROR: Failed to send reset password email, " + ex.Message);
        }
    }
}