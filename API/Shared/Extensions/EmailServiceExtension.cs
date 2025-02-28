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
    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Email services and settings are already registered in ConfigurationExtensions.AddAppConfiguration
        
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
}