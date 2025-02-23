using API.Data;
using API.Services;
using API.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using API.Repositories.Interfaces;
using API.Repositories;
using API.Services.Interfaces;
using API.Common.Utilities.Interfaces;
using API.Common.Helpers;

namespace API.Common.Utilities;

public class Services : IServices
{
    private readonly WebApplicationBuilder _builder;

    public Services(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    public void Initialize()
    {
        ConfigManager.AddConfiguration(_builder.Services, _builder.Configuration);
        AddDbContext(_builder);
        addUtils(_builder);
        AddServices(_builder);
        AddRepositories(_builder);
        InitializeHelpers(_builder.Configuration);
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<ISessionService, SessionService>();
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
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IVerificationRepository, VerificationRepository>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add repositories: {ex.Message}");
        }
    }

    private static void AddDbContext(WebApplicationBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment == "Testing") return;
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

    private static void addUtils(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEmailSender, EmailSender>();
    }

    private static void InitializeHelpers(IConfiguration configuration)
    {
        try
        {
            JWT.Initialize(configuration);
            Hasher.Initialize(configuration);
            Snowflake.Initialize(configuration);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize helpers: {ex.Message}");
        }
    }
}