using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using API.Configuration;
using API.Common.Utilities.Interfaces;

namespace API.Common.Utilities;

public class EmailSender : IEmailSender
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;
    private readonly string _frontendUrl;

    public EmailSender(IConfiguration configuration)
    {
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
        _frontendUrl = configuration.GetSection("Frontend").GetSection("BaseUrl").Value ?? "http://localhost:3000";
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = "Your OTP Code",
            Body = $"Your OTP code is: {otp}",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);

        try
        {
            await _smtpClient.SendMailAsync(mailMessage).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("ERROR: Failed to send OTP email, " + ex.Message);
        }
    }

    public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = "Reset Password",
            Body = $"Your reset password link is: {_frontendUrl}/app/auth/reset-password?code={codeHash}",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);

        try
        {
            await _smtpClient.SendMailAsync(mailMessage).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("ERROR: Failed to send reset password email, " + ex.Message);
        }
    }
}