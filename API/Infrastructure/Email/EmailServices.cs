using API.Shared.Interfaces.Email;
using API.Shared.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Infrastructure.Email;

/// <summary>
/// SMTP-based email service for production use
/// </summary>
public class SmtpEmailService : IEmailService
{
    protected readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public virtual async Task SendEmailAsync(string to, string subject, string body)
    {
        using var smtpClient = new SmtpClient
        {
            Host = _settings.Host,
            Port = _settings.Port,
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = _settings.UseDefaultCredentials,
            Timeout = _settings.Timeout
        };

        if (!_settings.UseDefaultCredentials && 
            !string.IsNullOrEmpty(_settings.Username) && 
            !string.IsNullOrEmpty(_settings.Password))
        {
            smtpClient.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        using var mailMessage = new MailMessage
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        // Use from email/name from settings or fallback to defaults
        string fromEmail = string.IsNullOrEmpty(_settings.DefaultFromEmail) 
            ? "noreply@example.com" 
            : _settings.DefaultFromEmail;
            
        string fromName = string.IsNullOrEmpty(_settings.DefaultFromName)
            ? "Authentication Service"
            : _settings.DefaultFromName;
            
        mailMessage.From = new MailAddress(fromEmail, fromName);
        mailMessage.To.Add(to);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
} 