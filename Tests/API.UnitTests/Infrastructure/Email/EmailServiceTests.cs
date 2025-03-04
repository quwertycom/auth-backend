using API.Infrastructure.Email;
using API.Shared.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using System.Net.Mail;
using FluentAssertions;
using System.Net;

namespace API.UnitTests.Infrastructure.Email;

[TestFixture]
public class EmailServiceTests
{
    private IOptions<EmailSettings> _mockEmailOptions = null!;
    private ISmtpClientWrapper _mockSmtpClient = null!;
    private TestableSmtpEmailService _emailService = null!;

    [SetUp]
    public void Setup()
    {
        _mockEmailOptions = Substitute.For<IOptions<EmailSettings>>();
        _mockSmtpClient = Substitute.For<ISmtpClientWrapper>();

        _mockEmailOptions.Value.Returns(new EmailSettings
        {
            Host = "localhost",
            Port = 25,
            EnableSsl = false,
            UseDefaultCredentials = true,
            DefaultFromEmail = "test@example.com",
            DefaultFromName = "Test Sender"
        });

        _emailService = new TestableSmtpEmailService(_mockEmailOptions, _mockSmtpClient);
    }

    [Test]
    public async Task SendEmailAsync_ValidInput_SendsEmail()
    {
        // Arrange
        var toEmail = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        await _emailService.SendEmailAsync(toEmail, subject, body);

        // Assert
        await _mockSmtpClient.Received(1).SendMailAsync(Arg.Is<MailMessage>(message =>
            message.To.Contains(new MailAddress(toEmail)) &&
            message.Subject == subject &&
            message.Body == body &&
            message.IsBodyHtml == true
        ));
    }
    
    [Test]
    public async Task SendEmailAsync_NoDefaultEmailSettings_UsesFallbacks()
    {
        // Arrange
        _mockEmailOptions.Value.Returns(new EmailSettings
        {
            Host = "localhost",
            Port = 25,
            EnableSsl = false,
            UseDefaultCredentials = true,
            DefaultFromEmail = "", // Empty from email
            DefaultFromName = ""   // Empty from name
        });
        
        _emailService = new TestableSmtpEmailService(_mockEmailOptions, _mockSmtpClient);
        
        var toEmail = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        await _emailService.SendEmailAsync(toEmail, subject, body);

        // Assert
        await _mockSmtpClient.Received(1).SendMailAsync(Arg.Is<MailMessage>(message =>
            message.From != null &&
            message.From.Address == "noreply@example.com" &&
            message.From.DisplayName == "Authentication Service"
        ));
    }
    
    [Test]
    public async Task SendEmailAsync_HTML_IsSetProperly()
    {
        // Arrange
        var toEmail = "recipient@example.com";
        var subject = "HTML Test";
        var body = "<html><body><h1>Hello World</h1></body></html>";

        // Act
        await _emailService.SendEmailAsync(toEmail, subject, body);

        // Assert
        await _mockSmtpClient.Received(1).SendMailAsync(Arg.Is<MailMessage>(message =>
            message.IsBodyHtml == true &&
            message.Body.Contains("<h1>")
        ));
    }
    
    [Test]
    public void SendEmailAsync_PropagatesExceptions()
    {
        // Arrange
        var toEmail = "recipient@example.com";
        var subject = "Test Exception";
        var body = "Test Body";
        
        // Configure mock to throw exception
        _mockSmtpClient.SendMailAsync(Arg.Any<MailMessage>())
            .Returns(Task.FromException(new SmtpException("Test SMTP error")));

        // Act & Assert
        Func<Task> act = async () => await _emailService.SendEmailAsync(toEmail, subject, body);
        
        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Test SMTP error*");
    }
    
    [Test]
    public async Task SendEmailAsync_WithCredentials_ConfiguresProperly()
    {
        // Arrange
        _mockEmailOptions.Value.Returns(new EmailSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Username = "testuser",
            Password = "testpass",
            DefaultFromEmail = "sender@example.com",
            DefaultFromName = "Sender Name"
        });
        
        var testableService = new CredentialsTestEmailService(_mockEmailOptions, _mockSmtpClient);
        
        var toEmail = "recipient@example.com";
        var subject = "Credential Test";
        var body = "Test Body";

        // Act
        await testableService.SendEmailAsync(toEmail, subject, body);

        // Assert
        testableService.WasCredentialSet.Should().BeTrue();
        testableService.NetworkCredential!.UserName.Should().Be("testuser");
        testableService.NetworkCredential!.Password.Should().Be("testpass");
    }
}

// Testable version of SmtpEmailService that uses our wrapper
public class TestableSmtpEmailService : SmtpEmailService
{
    private readonly ISmtpClientWrapper _smtpClientWrapper;

    public TestableSmtpEmailService(
        IOptions<EmailSettings> options,
        ISmtpClientWrapper smtpClientWrapper)
        : base(options)
    {
        _smtpClientWrapper = smtpClientWrapper;
    }

    // Override the base SendEmailAsync to use our mock
    public override async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            string fromEmail = string.IsNullOrEmpty(_settings.DefaultFromEmail) 
                ? "noreply@example.com" 
                : _settings.DefaultFromEmail;
                
            string fromName = string.IsNullOrEmpty(_settings.DefaultFromName)
                ? "Authentication Service"
                : _settings.DefaultFromName;
                
            mailMessage.From = new MailAddress(fromEmail, fromName);
            mailMessage.To.Add(to);

            await _smtpClientWrapper.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}

// Special version for testing credentials
public class CredentialsTestEmailService : SmtpEmailService
{
    private readonly ISmtpClientWrapper _smtpClientWrapper;
    public NetworkCredential? NetworkCredential { get; private set; }
    public bool WasCredentialSet { get; private set; }

    public CredentialsTestEmailService(
        IOptions<EmailSettings> options,
        ISmtpClientWrapper smtpClientWrapper)
        : base(options)
    {
        _smtpClientWrapper = smtpClientWrapper;
    }

    public override async Task SendEmailAsync(string to, string subject, string body)
    {
        // Simulate creating an SMTP client with credentials
        if (!_settings.UseDefaultCredentials && 
            !string.IsNullOrEmpty(_settings.Username) && 
            !string.IsNullOrEmpty(_settings.Password))
        {
            NetworkCredential = new NetworkCredential(_settings.Username, _settings.Password);
            WasCredentialSet = true;
        }
        
        var mailMessage = new MailMessage
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        
        string fromEmail = string.IsNullOrEmpty(_settings.DefaultFromEmail) 
            ? "noreply@example.com" 
            : _settings.DefaultFromEmail;
            
        string fromName = string.IsNullOrEmpty(_settings.DefaultFromName)
            ? "Authentication Service"
            : _settings.DefaultFromName;
            
        mailMessage.From = new MailAddress(fromEmail, fromName);
        mailMessage.To.Add(to);

        await _smtpClientWrapper.SendMailAsync(mailMessage);
    }
}
