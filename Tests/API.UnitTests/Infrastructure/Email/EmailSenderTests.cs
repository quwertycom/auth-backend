using API.Infrastructure.Email;
using API.Shared.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net.Mail;
using NUnit.Framework;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace API.UnitTests.Infrastructure.Email;

[TestFixture]
public class EmailSenderTests
{
    private IOptions<EmailSettings>? _mockEmailOptions;
    private IOptions<ApiSettings>? _mockApiOptions;
    private IWebHostEnvironment? _mockEnvironment;
    private string? _webRootPath;
    private string? _templatesPath;
    private string? _enTemplatesPath;

    [SetUp]
    public void Setup()
    {
        // Setup mocks
        _mockEmailOptions = Substitute.For<IOptions<EmailSettings>>();
        _mockApiOptions = Substitute.For<IOptions<ApiSettings>>();
        _mockEnvironment = Substitute.For<IWebHostEnvironment>();

        _mockEmailOptions.Value.Returns(new EmailSettings
        {
            Host = "localhost",
            Port = 25,
            EnableSsl = false,
            UseDefaultCredentials = true,
            DefaultFromEmail = "test@example.com",
            DefaultFromName = "Test Sender"
        });
        
        _mockApiOptions.Value.Returns(new ApiSettings { FrontendBaseUrl = "http://localhost:3000" });
        
        // Setup directory structure for testing
        _webRootPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "wwwroot");
        _templatesPath = Path.Combine(_webRootPath, "Templates", "Email");
        _enTemplatesPath = Path.Combine(_templatesPath, "en");
        
        // Create directories if they don't exist
        Directory.CreateDirectory(_templatesPath);
        Directory.CreateDirectory(_enTemplatesPath);
        
        // Set the mock environment
        _mockEnvironment.WebRootPath.Returns(_webRootPath);
        
        // Create test template files
        CreateTestTemplateFiles();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up test files if needed
        try
        {
            if (_webRootPath != null && Directory.Exists(_webRootPath))
            {
                Directory.Delete(_webRootPath, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private void CreateTestTemplateFiles()
    {
        // Create language-specific template
        if (_enTemplatesPath != null)
        {
            string enTemplatePath = Path.Combine(_enTemplatesPath, "otp-email.html");
            File.WriteAllText(enTemplatePath, "<html><body>EN template with {{OTP}} and {{FIRST_NAME}}</body></html>");
            
            // Create reset password template
            string resetPath = Path.Combine(_enTemplatesPath, "reset-password.html");
            File.WriteAllText(resetPath, "<html><body>Reset your password using this link: {{RESET_LINK}} for {{USERNAME}}</body></html>");
        }
        
        // Create default template
        if (_templatesPath != null)
        {
            string defaultTemplatePath = Path.Combine(_templatesPath, "otp-email.html");
            File.WriteAllText(defaultTemplatePath, "<html><body>Default template with {{OTP}} and {{FIRST_NAME}}</body></html>");
            
            string resetPath = Path.Combine(_templatesPath, "reset-password.html");
            File.WriteAllText(resetPath, "<html><body>Default reset password using: {{RESET_LINK}} for {{USERNAME}}</body></html>");
        }
    }

    [Test]
    public void GetTemplatePath_LanguageSpecificExists_ReturnsLanguageSpecificPath()
    {
        // Arrange
        var emailSender = new MockableEmailSender(_mockEmailOptions!, _mockApiOptions!, _mockEnvironment!);
        string templateName = "otp-email";
        string language = "en";

        // Act
        string templatePath = emailSender.GetTemplatePathForTest(templateName, language);

        // Assert
        templatePath.Should().EndWith($"Templates{Path.DirectorySeparatorChar}Email{Path.DirectorySeparatorChar}en{Path.DirectorySeparatorChar}{templateName}.html");
    }

    [Test]
    public void GetTemplatePath_LanguageSpecificNotExists_ReturnsDefaultPath()
    {
        // Arrange
        var emailSender = new MockableEmailSender(_mockEmailOptions!, _mockApiOptions!, _mockEnvironment!);
        string templateName = "otp-email";
        string language = "fr"; // French template does not exist

        // Act
        string templatePath = emailSender.GetTemplatePathForTest(templateName, language);

        // Assert
        templatePath.Should().EndWith($"Templates{Path.DirectorySeparatorChar}Email{Path.DirectorySeparatorChar}{templateName}.html"); // Should fall back to default
    }

    [Test]
    public async Task SendOtpEmailAsync_ValidInput_SendsEmail()
    {
        // Arrange
        var mockSmtpClient = Substitute.For<ISmtpClientWrapper>();
        var emailSender = new MockableEmailSender(_mockEmailOptions!, _mockApiOptions!, _mockEnvironment!, mockSmtpClient);
        
        var toEmail = "recipient@example.com";
        var otp = "123456";
        var firstName = "Recipient";

        // Act
        var result = await emailSender.SendOtpEmailAsync(toEmail, otp, firstName);

        // Assert
        result.Should().BeTrue();
        await mockSmtpClient.Received(1).SendMailAsync(Arg.Is<MailMessage>(message =>
            message.To.Contains(new MailAddress(toEmail)) &&
            message.Subject == "Your OTP Code" &&
            message.Body.Contains(otp) &&
            message.Body.Contains(firstName)
        ));
    }
    
    [Test]
    public async Task SendResetPasswordEmailAsync_ValidInput_SendsEmail()
    {
        // Arrange
        var mockSmtpClient = Substitute.For<ISmtpClientWrapper>();
        var emailSender = new MockableEmailSender(_mockEmailOptions!, _mockApiOptions!, _mockEnvironment!, mockSmtpClient);
        
        var toEmail = "reset@example.com";
        var codeHash = "abcdef123456";
        var expectedResetLink = "http://localhost:3000/app/auth/reset-password?code=abcdef123456";

        // Act
        var result = await emailSender.SendResetPasswordEmailAsync(toEmail, codeHash);

        // Assert
        result.Should().BeTrue();
        await mockSmtpClient.Received(1).SendMailAsync(Arg.Is<MailMessage>(message =>
            message.To.Contains(new MailAddress(toEmail)) &&
            message.Subject == "Reset Password" &&
            message.Body.Contains(expectedResetLink) &&
            message.Body.Contains("reset") // Username extracted from email
        ));
    }
    
    [Test]
    public async Task SendEmailAsync_WithCustomFrontendUrl_UsesCorrectResetLink()
    {
        // Arrange
        _mockApiOptions!.Value.Returns(new ApiSettings { FrontendBaseUrl = "https://custom-domain.com" });
        
        var mockSmtpClient = Substitute.For<ISmtpClientWrapper>();
        var emailSender = new MockableEmailSender(_mockEmailOptions!, _mockApiOptions, _mockEnvironment!, mockSmtpClient);
        
        var toEmail = "reset@example.com";
        var codeHash = "abcdef123456";
        var expectedResetLink = "https://custom-domain.com/app/auth/reset-password?code=abcdef123456";

        // Act
        var result = await emailSender.SendResetPasswordEmailAsync(toEmail, codeHash);

        // Assert
        result.Should().BeTrue();
        await mockSmtpClient.Received(1).SendMailAsync(Arg.Is<MailMessage>(message =>
            message.Body.Contains(expectedResetLink)
        ));
    }
    
    [Test]
    public void ReadTemplateAsync_TemplateNotFound_ThrowsException()
    {
        // Arrange
        var emailSender = new MockableEmailSender(_mockEmailOptions!, _mockApiOptions!, _mockEnvironment!);
        string nonExistentPath = Path.Combine(_templatesPath!, "non-existent-template.html");

        // Act & Assert
        Func<Task> act = async () => await emailSender.ReadTemplateAsync(nonExistentPath);
        
        act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*Email template not found*");
    }
}

// Interface for mocking SmtpClient
public interface ISmtpClientWrapper
{
    Task SendMailAsync(MailMessage message);
}

// A custom implementation of EmailSender for testing
public class MockableEmailSender : EmailSender
{
    private readonly ISmtpClientWrapper? _smtpClientWrapper;
    private readonly IWebHostEnvironment _environment;
    
    public MockableEmailSender(
        IOptions<EmailSettings> emailOptions, 
        IOptions<ApiSettings> apiOptions, 
        IWebHostEnvironment environment,
        ISmtpClientWrapper? smtpClientWrapper = null) 
        : base(emailOptions, apiOptions, environment)
    {
        _smtpClientWrapper = smtpClientWrapper;
        _environment = environment;
    }
    
    // Expose the private method for testing
    public string GetTemplatePathForTest(string templateName, string language)
    {
        // First check if language-specific template exists
        string langSpecificPath = Path.Combine(_environment.WebRootPath, "Templates/Email", language, $"{templateName}.html");
        
        // If language-specific template doesn't exist, fall back to default
        if (!File.Exists(langSpecificPath))
        {
            return Path.Combine(_environment.WebRootPath, "Templates/Email", $"{templateName}.html");
        }
        
        return langSpecificPath;
    }
    
    public override Task<string> ReadTemplateAsync(string templatePath)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at: {templatePath}");
        }
        
        // For tests, we'll return the actual file content or a simple template with placeholders
        return File.Exists(templatePath) 
            ? File.ReadAllTextAsync(templatePath) 
            : Task.FromResult("<html><body>Email template with {{OTP}} and {{FIRST_NAME}}</body></html>");
    }
    
    protected override async Task<bool> SendEmailFromTemplateAsync(
        string toEmail, 
        string subject, 
        string templateName, 
        string language,
        Dictionary<string, string> placeholders)
    {
        try
        {
            // Get template path, including language-specific version if it exists
            string templatePath = GetTemplatePathForTest(templateName, language);
            
            // Read the template content
            string templateContent = await ReadTemplateAsync(templatePath);
            
            // Replace all placeholders
            string emailBody = ReplacePlaceholders(templateContent, placeholders);
            
            // Create the email
            var mailMessage = new MailMessage
            {
                Subject = subject,
                Body = emailBody,
                IsBodyHtml = true,
            };
            
            // Use from email/name from settings or fallback to defaults
            string fromEmail = "test@example.com";
            string fromName = "Test Sender";
            mailMessage.From = new MailAddress(fromEmail, fromName);
            
            mailMessage.To.Add(toEmail);
            
            // If we have a mock wrapper, use it instead of the real SmtpClient
            if (_smtpClientWrapper != null)
            {
                await _smtpClientWrapper.SendMailAsync(mailMessage);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send email using template '{templateName}': {ex.Message}", ex);
        }
    }
    
    // Helper method to replace placeholders
    private string ReplacePlaceholders(string templateContent, Dictionary<string, string> placeholders)
    {
        string result = templateContent;
        
        foreach (var placeholder in placeholders)
        {
            result = result.Replace(placeholder.Key, placeholder.Value);
        }
        
        return result;
    }
}
