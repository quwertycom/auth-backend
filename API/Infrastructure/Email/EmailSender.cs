using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using API.Shared.Configuration;
using API.Shared.Interfaces.Email;

namespace API.Infrastructure.Email;

/// <summary>
/// Sends emails using SMTP client with template support.
/// </summary>
public class EmailSender : IEmailSender, IDisposable
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl;
    private readonly IWebHostEnvironment _environment;
    private const string TemplatesPath = "Templates/Email";

    public EmailSender(
        IOptions<EmailSettings> emailOptions, 
        IOptions<ApiSettings> apiOptions,
        IWebHostEnvironment environment)
    {
        var settings = emailOptions.Value;
        _environment = environment;

        _smtpClient = new SmtpClient
        {
            Host = settings.Host,
            Port = settings.Port,
            EnableSsl = settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = settings.Timeout,
            UseDefaultCredentials = settings.UseDefaultCredentials
        };
        
        if (!settings.UseDefaultCredentials && 
            !string.IsNullOrEmpty(settings.Username) && 
            !string.IsNullOrEmpty(settings.Password))
        {
            _smtpClient.Credentials = new NetworkCredential(settings.Username, settings.Password);
        }

        _fromEmail = settings.DefaultFromEmail;
        _fromName = settings.DefaultFromName;
        _frontendUrl = apiOptions.Value.FrontendBaseUrl ?? "http://localhost:3000";
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otp, string firstName, string language = "en")
    {
        var templateData = new Dictionary<string, string>
        {
            { "{{OTP}}", otp },
            { "{{FIRST_NAME}}", firstName ?? toEmail.Split('@')[0] }
        };
        
        return await SendEmailFromTemplateAsync(
            toEmail, 
            "Your OTP Code", 
            "otp-email", 
            language,
            templateData);
    }

    public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash, string language = "en")
    {
        var resetLink = $"{_frontendUrl}/app/auth/reset-password?code={codeHash}";
        
        var templateData = new Dictionary<string, string>
        {
            { "{{RESET_LINK}}", resetLink },
            { "{{USERNAME}}", toEmail.Split('@')[0] }
        };
        
        return await SendEmailFromTemplateAsync(
            toEmail, 
            "Reset Password", 
            "reset-password", 
            language, 
            templateData);
    }
    
    /// <summary>
    /// Sends an email using a template with placeholders.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="templateName">Name of the template without extension</param>
    /// <param name="language">Language code (e.g. 'en', 'fr')</param>
    /// <param name="placeholders">Dictionary of placeholders and their values</param>
    /// <returns>True if email was sent successfully</returns>
    protected virtual async Task<bool> SendEmailFromTemplateAsync(
        string toEmail, 
        string subject, 
        string templateName, 
        string language,
        Dictionary<string, string> placeholders)
    {
        try
        {
            // Get template path, including language-specific version if it exists
            string templatePath = GetTemplatePath(templateName, language);
            
            // Read the template content
            string templateContent = await ReadTemplateAsync(templatePath);
            
            // Replace all placeholders
            string emailBody = ReplacePlaceholders(templateContent, placeholders);
            
            // Create and send the email
            var mailMessage = new MailMessage
            {
                Subject = subject,
                Body = emailBody,
                IsBodyHtml = true,
            };
            
            // Use from email/name from settings or fallback to defaults
            string fromEmail = string.IsNullOrEmpty(_fromEmail) ? "noreply@example.com" : _fromEmail;
            string fromName = string.IsNullOrEmpty(_fromName) ? "Authentication Service" : _fromName;
            mailMessage.From = new MailAddress(fromEmail, fromName);
            
            mailMessage.To.Add(toEmail);
            
            await _smtpClient.SendMailAsync(mailMessage).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send email using template '{templateName}': {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the path to the template, with language-specific version if available.
    /// </summary>
    private string GetTemplatePath(string templateName, string language)
    {
        // First check if language-specific template exists
        string langSpecificPath = Path.Combine(_environment.WebRootPath, TemplatesPath, language, $"{templateName}.html");
        
        // If language-specific template doesn't exist, fall back to default
        if (!File.Exists(langSpecificPath))
        {
            return Path.Combine(_environment.WebRootPath, TemplatesPath, $"{templateName}.html");
        }
        
        return langSpecificPath;
    }
    
    /// <summary>
    /// Reads the template content from file.
    /// </summary>
    public virtual async Task<string> ReadTemplateAsync(string templatePath)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at: {templatePath}");
        }
        
        using var reader = new StreamReader(templatePath, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
    
    /// <summary>
    /// Replaces all placeholders in the template with their values.
    /// </summary>
    private string ReplacePlaceholders(string templateContent, Dictionary<string, string> placeholders)
    {
        string result = templateContent;
        
        foreach (var placeholder in placeholders)
        {
            result = result.Replace(placeholder.Key, placeholder.Value);
        }
        
        return result;
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}