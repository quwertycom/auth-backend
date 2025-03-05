using API.Shared.Interfaces.Email;

namespace API.IntegrationTests.Mocks;

/// <summary>
/// Mock implementation of IEmailSender for testing purposes that doesn't actually send emails
/// </summary>
public class MockEmailSender : IEmailSender
{
    public Task<bool> SendOtpEmailAsync(string toEmail, string otp, string firstName, string language = "en")
    {
        // Log or store the email for testing rather than sending
        return Task.FromResult(true);
    }

    public Task<bool> SendResetPasswordEmailAsync(string toEmail, string codeHash, string language = "en")
    {
        // Log or store the email for testing rather than sending
        return Task.FromResult(true);
    }
}