using API.Data;
using API.Services;
using API.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using API.Repositories.Interfaces;
using API.Repositories;
using API.Services.Interfaces;

namespace API.Common.Helpers;

public static class Services
{
    public static void Initialize(WebApplicationBuilder builder)
    {
        ConfigManager.AddConfiguration(builder.Services, builder.Configuration);
        AddDbContext(builder);
        AddServices(builder);
        AddRepositories(builder);
        InitializeHelpers(builder.Configuration);
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add services: {ex.Message}");
        }
    }

    private static void AddRepositories(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add repositories: {ex.Message}");
        }
    }

    private static void AddDbContext(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
            {
                var isRunningInDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING")?.ToLower() == "true";

                var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = isRunningInDocker ? "db" : Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                    Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? throw new ArgumentNullException("POSTGRES_DB"),
                    Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? throw new ArgumentNullException("POSTGRES_USER"),
                    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? throw new ArgumentNullException("POSTGRES_PASSWORD"),
                    Pooling = true,
                    MinPoolSize = 5,
                    MaxPoolSize = 100
                };

                options.UseNpgsql(connectionStringBuilder.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add db context: {ex.Message}");
        }
    }

    private static void InitializeHelpers(IConfiguration configuration)
    {
        try
        {
            var initializationTasks = new Dictionary<string, Action>
            {
                { "JWT", () => JWT.Initialize(configuration) },
                { "PasswordHasher", () => PasswordHasher.Initialize(configuration) },
                { "Snowflake", () => Snowflake.Initialize(configuration) },
                { "EmailSender", () => EmailSender.Initialize(configuration) }
            };

            foreach (var task in initializationTasks)
            {
                try
                {
                    task.Value();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to initialize {task.Key} helper: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize helpers: {ex.Message}");
        }
    }
}