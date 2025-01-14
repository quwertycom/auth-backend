using API.Data;
using API.Service;
using Microsoft.EntityFrameworkCore;

namespace API.Common.Helpers;

public static class Services
{
    public static void Initialize(WebApplicationBuilder builder)
    {
        AddDbContext(builder);
        AddControllerServices(builder);
        InitializeHelpers(builder);
    }

    private static void AddControllerServices(WebApplicationBuilder builder) {
        try {
            builder.Services.AddScoped<IAuthService, AuthService>();
            // add other services in the future
        } catch {
            throw new Exception("Failed to add controller services");
        }
    }

    private static void AddDbContext(WebApplicationBuilder builder) {
        try {
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
        } catch {
            throw new Exception("Failed to add db context");
        }
    }

    private static void InitializeHelpers(WebApplicationBuilder builder) {
        try {
            // Initialize JWT helper
            JWT.Initialize(builder.Configuration, builder.Environment);

            // Initialize Snowflake helper
            Snowflake.Initialize(builder.Configuration, builder.Environment);

            // Initialize PasswordHasher helper
            PasswordHasher.Initialize(builder.Configuration, builder.Environment);
        } catch {
            throw new Exception("Failed to initialize helpers");
        }
    }
}