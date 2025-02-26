using API.Common.Helpers;
using API.Configuration;
using API.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace API.Extensions;

/// <summary>
/// Main extension methods for configuring the application's services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all application services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration services
        ConfigManager.AddConfiguration(services, configuration);
        
        // Add various service groups using proper extension methods
        services
            .AddDatabaseServices()
            .AddUtilityServices()
            .AddBusinessServices()
            .AddRepositoryServices();
        
        // Register helper initialization hosted services
        services.AddHostedServices();
        
        return services;
    }
    
    /// <summary>
    /// Adds hosted services for initializing helper classes
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    private static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        try
        {
            // Register hosted services for helper initialization
            services.AddHostedService<HasherInitializationService>();
            services.AddHostedService<SnowflakeInitializationService>();
            
            return services;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to register helper hosted services: {ex.Message}");
        }
    }
} 