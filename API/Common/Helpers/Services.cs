using API.Data;
using API.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Common.Helpers;

public static class Services
{
    public static void Initialize(WebApplicationBuilder builder)
    {
        AddDbContext(builder);
        AddControllerServices(builder);
        InitializeHelpers(builder.Configuration);
    }

    private static void AddControllerServices(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddScoped<IAuthService, AuthService>();
            // add other services in the future
        }
        catch
        {
            throw new Exception("Failed to add controller services");
        }
    }

    private static void AddDbContext(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddDbContext<AuthDbContext>(options =>
            {
                var isRunningInDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING")?.ToLower() == "true";
                var host = isRunningInDocker ? "db" : "localhost";

                var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Database = builder.Configuration["POSTGRES_DB"],
                    Username = builder.Configuration["POSTGRES_USER"],
                    Password = builder.Configuration["POSTGRES_PASSWORD"],
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
        catch
        {
            throw new Exception("Failed to add db context");
        }
    }

    private static void InitializeHelpers(IConfiguration configuration)
    {
        try
        {
            // Initialize JWT helper
            try
            {
                JWT.Initialize(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize JWT helper: {ex.Message}");
            }

            // Initialize PasswordHasher helper
            try
            {
                PasswordHasher.Initialize(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize PasswordHasher helper: {ex.Message}");
            }

            // Initialize Snowflake helper
            try
            {
                Snowflake.Initialize(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize Snowflake helper: {ex.Message}");
            }

            // Initialize EmailSender helper
            try
            {
                EmailSender.Initialize(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize EmailSender helper: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize helpers: {ex.Message}");
        }
    }
}