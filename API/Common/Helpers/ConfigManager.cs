using API.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Extensions.Options;

namespace API.Common.Helpers;

public static class ConfigManager
{
    private static readonly string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "not-found";
    public static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        if (envName != "not-found")
        {
            switch (envName)
            {
                case "Testing":
                    builder.AddJsonFile("appsettings.Testing.json", optional: false);
                    break;
                case "Development":
                    builder.AddJsonFile("appsettings.Development.json", optional: false);
                    builder.AddEnvironmentVariables();
                    break;
                case "Production":
                    builder.AddJsonFile("appsettings.Production.json", optional: false);
                    builder.AddEnvironmentVariables();
                    break;
            }
        }
        
        return builder.Build();
    }

    public static void AddConfiguration(IServiceCollection services, IConfiguration? configuration = null)
    {
        configuration ??= GetConfiguration();
        
        // Proper options registration with validation
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
        var fullConfig = configuration.AsEnumerable().ToDictionary(k => k.Key, v => v.Value);
        string configString = System.Text.Json.JsonSerializer.Serialize(fullConfig);

        if (envName != "Testing")
        {
            // Database settings validation
            var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
            var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
            var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
            var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

            if (string.IsNullOrEmpty(dbHost)) throw new InvalidOperationException($"POSTGRES_HOST not configured. Full configuration: {configString}");
            if (string.IsNullOrEmpty(dbName)) throw new InvalidOperationException($"POSTGRES_DB not configured. Full configuration: {configString}");
            if (string.IsNullOrEmpty(dbUser)) throw new InvalidOperationException($"POSTGRES_USER not configured. Full configuration: {configString}");
            if (string.IsNullOrEmpty(dbPassword)) throw new InvalidOperationException($"POSTGRES_PASSWORD not configured. Full configuration: {configString}");
        }

        // JWT settings validation
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null)
        {
            throw new InvalidOperationException($"JWT configuration is missing. env: {envName}. Full configuration: {configString}");
        }

        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
            throw new InvalidOperationException($"JWT secret key is not configured. Full configuration: {configString}");

        if (jwtSettings.SecretKey.Length < 32)
            throw new InvalidOperationException($"JWT secret key must be at least 32 characters long. Full configuration: {configString}");

        // Email settings validation
        var emailSettings = configuration.GetSection("Email").Get<EmailSettings>();
        if (emailSettings == null)
            throw new InvalidOperationException($"Email configuration is missing. Full configuration: {configString}");

        if (string.IsNullOrEmpty(emailSettings.Username))
            throw new InvalidOperationException($"Email username is not configured. Full configuration: {configString}");

        if (string.IsNullOrEmpty(emailSettings.Password))
            throw new InvalidOperationException($"Email password is not configured. Full configuration: {configString}");

        if (string.IsNullOrEmpty(emailSettings.FromEmail))
            throw new InvalidOperationException($"Email from address is not configured. Full configuration: {configString}");
    }

    private static void ValidateJwtSettings(JwtSettings settings)
    {
        if (Encoding.UTF8.GetByteCount(settings.SecretKey) < 32)
            throw new OptionsValidationException("Jwt:SecretKey", 
                typeof(JwtSettings), 
                new[] { "Secret key must be at least 32 bytes (256 bits)" });
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