using API.Shared.Configuration;
using API.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotNetEnv;
using System.IO;
using Microsoft.Extensions.Hosting;
using System.Net.NetworkInformation;

namespace API.Shared.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds and configures application configuration, including environment variables
    /// </summary>
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services, configuration);
        LoadEnvironmentVariables(services);
        return services;
    }
    
    private static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<ApiSettings>(configuration.GetSection("Api"));
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        services.Configure<PasswordHasherSettings>(configuration.GetSection("PasswordHasher"));
        services.Configure<SnowflakeSettings>(configuration.GetSection("Snowflake"));
    }

    private static void LoadEnvironmentVariables(this IServiceCollection services)
    {
        Env.Load();


        services.PostConfigure<ApiSettings>(settings =>
        {
            settings.Port = Environment.GetEnvironmentVariable("API_PORT") ?? settings.Port.ToString();
        });
        services.PostConfigure<JwtSettings>(settings =>
        {
            settings.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? settings.SecretKey;
            settings.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? settings.Issuer;
            settings.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? settings.Audience;
        });
        services.PostConfigure<PasswordHasherSettings>(settings =>
        {
            settings.Iterations = int.Parse(Environment.GetEnvironmentVariable("PASSWORD_HASHER_ITERATIONS") ?? settings.Iterations.ToString());
            settings.SaltSize = int.Parse(Environment.GetEnvironmentVariable("PASSWORD_HASHER_SALT_SIZE") ?? settings.SaltSize.ToString());
            settings.KeySize = int.Parse(Environment.GetEnvironmentVariable("PASSWORD_HASHER_KEY_SIZE") ?? settings.KeySize.ToString());
        });
        services.PostConfigure<DatabaseSettings>(settings =>
        {
            settings.Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? settings.Host;
            settings.Database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? settings.Database;
            settings.Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? settings.Username;
            settings.Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? settings.Password;
        });
        services.PostConfigure<EmailSettings>(settings =>
        {
            settings.Host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? settings.Host;
            settings.Port = int.Parse(Environment.GetEnvironmentVariable("EMAIL_PORT") ?? settings.Port.ToString());
            settings.EnableSsl = bool.Parse(Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL") ?? settings.EnableSsl.ToString());
            settings.Username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? settings.Username;
            settings.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? settings.Password;
            settings.DefaultFromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_DEFAULT") ?? settings.DefaultFromEmail;
            settings.Timeout = int.Parse(Environment.GetEnvironmentVariable("EMAIL_TIMEOUT") ?? settings.Timeout.ToString());
            settings.UseDefaultCredentials = bool.Parse(Environment.GetEnvironmentVariable("EMAIL_USE_DEFAULT_CREDENTIALS") ?? settings.UseDefaultCredentials.ToString());
        });
    }
}