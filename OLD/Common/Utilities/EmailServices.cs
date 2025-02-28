using API.Common.Utilities.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace API.Common.Utilities;

public class LocalEmailService : IDeveloperEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await Task.CompletedTask;
    }
}

public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly IEnvironmentVariableProvider _env;

    public SendGridEmailService(IEnvironmentVariableProvider env)
    {
        _env = env;
        var apiKey = _env.GetVariable("SENDGRID_API_KEY") ?? throw new InvalidOperationException("SendGrid API key not found");
        _client = new SendGridClient(apiKey);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var from = new EmailAddress(_env.GetVariable("SENDGRID_FROM_EMAIL") ?? "noreply@example.com");
        var msg = MailHelper.CreateSingleEmail(from, new EmailAddress(to), subject, body, body);
        await _client.SendEmailAsync(msg);
    }
} 