using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace API.Common.Helpers;

public static class EmailSender
{
    private static SmtpClient? _smtpClient;
    private static string? _fromEmail;
    private static bool _isInitialized = false;

    public static void Initialize(IConfiguration configuration)
    {
        if (_isInitialized) return;

        // Load configuration values directly
        _smtpClient = new SmtpClient
        {
            Host = configuration["Email:Host"] ?? throw new InvalidOperationException("Email:Host is not configured"),
            Port = int.TryParse(configuration["Email:Port"], out var port) ? port : throw new InvalidOperationException("Email:Port is not a valid integer"),
            EnableSsl = bool.TryParse(configuration["Email:EnableSsl"], out var enableSsl) ? enableSsl : throw new InvalidOperationException("Email:EnableSsl is not a valid boolean"),
            Credentials = new NetworkCredential(
                configuration["Email:Username"],
                configuration["Email:Password"]
            )
        };
        _fromEmail = configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail is not configured");

        _isInitialized = true;
    }

    public static async Task SendOtpEmailAsync(string toEmail, string otp)
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
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error)
            throw new InvalidOperationException("Failed to send email", ex);
        }
    }
}