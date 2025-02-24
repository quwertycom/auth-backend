namespace API.Common.Utilities.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public interface IDeveloperEmailService : IEmailService { } 