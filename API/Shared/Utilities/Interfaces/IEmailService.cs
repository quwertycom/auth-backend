namespace API.Shared.Utilities.Interfaces;

/// <summary>
/// Interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to the specified email address.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body);
}

public interface IDeveloperEmailService : IEmailService { } 