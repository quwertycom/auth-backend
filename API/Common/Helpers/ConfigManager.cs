using API.Web.Configuration;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Common.Helpers;

public static class ConfigManager
{
    private static readonly string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "not-found";
    
    private static bool IsProductionLikeEnvironment => envName != "Development" && envName != "Testing";

    public static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        // In non-Development environments, prioritize environment variables
        if (envName != "Development")
        {
            builder.AddEnvironmentVariables();
        }

        builder
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true);

        // In Development, load environment variables last to allow overriding in appsettings.Development.json if needed for local testing.
        if (envName == "Development")
        {
            builder.AddEnvironmentVariables();
        }

        return builder.Build();
    }

    public static void AddConfiguration(IServiceCollection services, IConfiguration? configuration = null)
    {
        configuration ??= GetConfiguration();

        // Add required environment variables validation first
        if (IsProductionLikeEnvironment)
        {
            services.AddOptions<RequiredEnvironmentSettings>()
                .Configure(settings =>
                {
                    settings.PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? string.Empty;
                    settings.PostgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? string.Empty;
                    settings.PostgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? string.Empty;
                    settings.PostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? string.Empty;
                    settings.JwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? string.Empty;
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
        
        // Using consistent IOptions pattern for all configuration sections
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("Email"))
            .Validate(settings => 
                !string.IsNullOrEmpty(settings.Username) && 
                !string.IsNullOrEmpty(settings.Password) && 
                !string.IsNullOrEmpty(settings.FromEmail),
                "Email settings are incomplete")
            .ValidateOnStart();

        services.AddOptions<PasswordHasherSettings>()
            .Bind(configuration.GetSection("PasswordHasher"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SnowflakeSettings>()
            .Bind(configuration.GetSection("Snowflake"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        // Register new strongly-typed configuration classes
        services.AddOptions<ApiSettings>()
            .Bind(configuration.GetSection("ApiSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        services.AddOptions<RateLimitingSettings>()
            .Bind(configuration.GetSection("RateLimiting"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    public static HealthCheckResult ValidateEnvironmentVariables(IConfiguration configuration)
    {
        if (!IsProductionLikeEnvironment)
        {
            return HealthCheckResult.Healthy("Environment variable validation skipped in non-production environments");
        }

        try
        {
            var settings = new RequiredEnvironmentSettings
            {
                PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? string.Empty,
                PostgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? string.Empty,
                PostgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? string.Empty,
                PostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? string.Empty,
                JwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? string.Empty
            };

            var context = new ValidationContext(settings);
            var results = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(settings, context, results, true))
            {
                var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
                return HealthCheckResult.Unhealthy($"Missing or invalid environment variables: {errors}");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Error validating environment variables: {ex.Message}");
        }
    }
}

// Add validation attributes to settings classes
public class JwtSettings
{
    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = null!;
    
    [Required]
    public string Issuer { get; set; } = null!;
    
    [Required]
    public string Audience { get; set; } = null!;
}