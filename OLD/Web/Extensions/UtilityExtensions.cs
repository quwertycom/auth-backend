using API.Common.Utilities.Interfaces;
using API.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace API.Web.Extensions;

/// <summary>
/// Extension methods for utility service registration
/// </summary>
public static class UtilityExtensions
{
    /// <summary>
    /// Adds utility services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }
} 