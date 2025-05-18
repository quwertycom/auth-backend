using API.Infrastructure.Email;
using API.Shared.Configuration;
using API.Shared.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace API.Shared.Extensions;

/// <summary>
/// Extensions for configuring email services.
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Adds email services to the service collection.
    /// </summary>
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        // Validate configuration before registering service
        ValidateConfiguration(services);

        // Register email services
        services.AddScoped<IEmailSender, EmailSender>();

        // Register development or production email service based on configuration
        services.AddScoped<IEmailService>(serviceProvider =>
        {
            var emailSettings = serviceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

            return new SmtpEmailService(
              serviceProvider.GetRequiredService<IOptions<EmailSettings>>());
        });

        return services;
    }

    private static void ValidateConfiguration(this IServiceCollection services)
    {
        // Build a temporary service provider to access settings
        using var serviceProvider = services.BuildServiceProvider();
        var emailSettings = serviceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

        if (string.IsNullOrEmpty(emailSettings.Host))
        {
            throw new InvalidOperationException("Email host is not configured.");
        }

        if (emailSettings.Port <= 0 || emailSettings.Port > 65535)
        {
            throw new InvalidOperationException($"Email port is not configured or is invalid: {emailSettings.Port}. Port must be between 1 and 65535.");
        }

        if (string.IsNullOrEmpty(emailSettings.Username))
        {
            throw new InvalidOperationException("Email username is not configured.");
        }

        if (string.IsNullOrEmpty(emailSettings.Password))
        {
            throw new InvalidOperationException("Email password is not configured.");
        }

        if (string.IsNullOrEmpty(emailSettings.DefaultFromEmail))
        {
            throw new InvalidOperationException("Email default from address is not configured.");
        }

        if (emailSettings.Timeout <= 0)
        {
            throw new InvalidOperationException($"Email timeout is not configured or is invalid: {emailSettings.Timeout}. Timeout must be a positive value.");
        }
    }
}