using API.Shared.Configuration;
using API.Shared.Interfaces.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotNetEnv;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace API.Shared.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds and configures application configuration, including environment variables
    /// </summary>
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration settings with IOptions pattern
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<ApiSettings>(configuration.GetSection("Api"));
        services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));
        
        return services;
    }
    
    /// <summary>
    /// Loads environment variables from .env file in development and adds them to configuration with high priority
    /// </summary>
    public static IConfigurationBuilder AddDotEnvConfiguration(this IConfigurationBuilder builder, IHostEnvironment environment)
    {
        if (environment.IsDevelopment()) // Conditionally load .env only in development
        {
            // Find .env file location - check both project directory and solution root
            var projectDir = Directory.GetCurrentDirectory();
            var solutionDir = Directory.GetParent(projectDir)?.FullName;
            
            var projectEnvPath = Path.Combine(projectDir, ".env");
            var solutionEnvPath = solutionDir != null ? Path.Combine(solutionDir, ".env") : null;
            
            // Try loading from project directory first, then solution directory
            if (File.Exists(projectEnvPath))
            {
                Env.Load(projectEnvPath);
            }
            else if (solutionEnvPath != null && File.Exists(solutionEnvPath))
            {
                Env.Load(solutionEnvPath);
            }
        }
        
        // Add environment variables with highest priority
        builder.AddEnvironmentVariables();
        
        return builder;
    }
    
    /// <summary>
    /// Binds and validates configuration section to strongly typed options
    /// </summary>
    public static IServiceCollection AddConfigurationWithValidation<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration, 
        string sectionName) where TOptions : class, new()
    {
        // Register the configuration instance with validation
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        return services;
    }
    
    /// <summary>
    /// Extension method to get strongly typed configuration sections
    /// </summary>
    public static T GetTypedSection<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        var section = new T();
        configuration.GetSection(sectionName).Bind(section);
        return section;
    }
} 