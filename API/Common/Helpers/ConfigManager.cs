using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using API.Configuration;

namespace API.Common.Helpers;

public static class ConfigManager
{
    private static IConfiguration? _configuration;

    public static IConfiguration GetConfiguration(bool isDevelopment)
    {
        if (_configuration != null)
            return _configuration;

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{(isDevelopment ? "Development" : "Production")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true);

        _configuration = builder.Build();
        return _configuration;
    }

    public static void AddConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration sections as strongly-typed options
        services.Configure<DatabaseSettings>(
            configuration.GetSection("Database"));

        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt"));

        services.Configure<EmailSettings>(
            configuration.GetSection("Email"));

        services.Configure<PasswordHasherSettings>(
            configuration.GetSection("PasswordHasher"));

        services.Configure<SnowflakeSettings>(
            configuration.GetSection("Snowflake"));

        // Validate required configuration
        ValidateConfiguration(configuration);
    }

    private static void ValidateConfiguration(IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        bool isTestEnvironment = environment == "Test" || environment == "Testing" || environment == "IntegrationTests";

        if (!isTestEnvironment)
        {
            // Database settings validation
            var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
            var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
            var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
            var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

            if (string.IsNullOrEmpty(dbHost)) throw new InvalidOperationException("POSTGRES_HOST not configured");
            if (string.IsNullOrEmpty(dbName)) throw new InvalidOperationException("POSTGRES_DB not configured");
            if (string.IsNullOrEmpty(dbUser)) throw new InvalidOperationException("POSTGRES_USER not configured");
            if (string.IsNullOrEmpty(dbPassword)) throw new InvalidOperationException("POSTGRES_PASSWORD not configured");
        }

        // JWT settings validation
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null)
            throw new InvalidOperationException("JWT configuration is missing");

        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT secret key is not configured");

        if (jwtSettings.SecretKey.Length < 32)
            throw new InvalidOperationException("JWT secret key must be at least 32 characters long");

        // Email settings validation
        var emailSettings = configuration.GetSection("Email").Get<EmailSettings>();
        if (emailSettings == null)
            throw new InvalidOperationException("Email configuration is missing");

        if (string.IsNullOrEmpty(emailSettings.Username))
            throw new InvalidOperationException("Email username is not configured");

        if (string.IsNullOrEmpty(emailSettings.Password))
            throw new InvalidOperationException("Email password is not configured");

        if (string.IsNullOrEmpty(emailSettings.FromEmail))
            throw new InvalidOperationException("Email from address is not configured");
    }
}