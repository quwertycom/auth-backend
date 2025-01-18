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
        // Database settings validation
        var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
        if (dbSettings == null)
            throw new InvalidOperationException("Database configuration is missing");

        if (string.IsNullOrEmpty(dbSettings.Password))
            throw new InvalidOperationException("Database password is not configured");

        if (string.IsNullOrEmpty(dbSettings.Username))
            throw new InvalidOperationException("Database username is not configured");

        if (string.IsNullOrEmpty(dbSettings.Database))
            throw new InvalidOperationException("Database name is not configured");

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

        // Log configuration status (but not sensitive values)
        Console.WriteLine("\nConfiguration validated successfully:");
        Console.WriteLine($"Database Host: {dbSettings.Host}");
        Console.WriteLine($"Database Name: {dbSettings.Database}");
        Console.WriteLine($"JWT Issuer: {jwtSettings.Issuer}");
        Console.WriteLine($"JWT Audience: {jwtSettings.Audience}");
        Console.WriteLine($"Email Host: {emailSettings.Host}\n");
    }
}