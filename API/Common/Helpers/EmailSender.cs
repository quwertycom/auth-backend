using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace API.Common.Helpers;

public static class EmailSender
{
    private static SmtpClient? _smtpClient;
    private static string? _fromEmail;
    private static bool _isInitialized = false;

    public static void Initialize(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (_isInitialized) return;

        if (environment.IsDevelopment())
        {
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
            _fromEmail = configuration["Email:FromEmail"];
        }
        else
        {
            _smtpClient = new SmtpClient
            {
                Host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? throw new InvalidOperationException("EMAIL_HOST environment variable is not set"),
                Port = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_PORT"), out var port) ? port : throw new InvalidOperationException("EMAIL_PORT environment variable is not a valid integer"),
                EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL"), out var enableSsl) ? enableSsl : throw new InvalidOperationException("EMAIL_ENABLE_SSL environment variable is not a valid boolean"),
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("EMAIL_USERNAME"),
                    Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                )
            };
            _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM");
        }

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