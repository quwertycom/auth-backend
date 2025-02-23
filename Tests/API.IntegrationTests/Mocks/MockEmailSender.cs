
using API.Common.Utilities.Interfaces;

namespace API.IntegrationTests.Mocks;

public class MockEmailSender : IEmailSender
{
    public Task<bool> SendEmailAsync(string email, string subject, string message) => Task.FromResult(true);

    public Task<bool> SendOtpEmailAsync(string email, string otp) => Task.FromResult(true);

    public Task<bool> SendResetPasswordEmailAsync(string email, string token) => Task.FromResult(true);
}
