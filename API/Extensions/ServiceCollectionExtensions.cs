using API.Common.Helpers;
using API.Configuration;
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
        
        // Initialize helpers
        InitializeHelpers(services);
        
        return services;
    }
    
    private static void InitializeHelpers(IServiceCollection services)
    {
        try
        {
            // Create service provider to resolve IOptions from the container
            var serviceProvider = services.BuildServiceProvider();
            
            // Initialize helpers with IOptions
            var passwordHasherOptions = serviceProvider.GetRequiredService<IOptions<PasswordHasherSettings>>();
            var snowflakeOptions = serviceProvider.GetRequiredService<IOptions<SnowflakeSettings>>();
            
            Hasher.Initialize(passwordHasherOptions);
            Snowflake.Initialize(snowflakeOptions);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize helpers: {ex.Message}");
        }
    }
} 