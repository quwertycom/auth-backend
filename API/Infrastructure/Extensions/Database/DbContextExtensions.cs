using API.Data;
using API.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace API.Extensions;

/// <summary>
/// Extension methods for database context registration
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Adds and configures the AuthDbContext to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddAuthDbContext(this IServiceCollection services)
    {
        try
        {
            services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
            {
                var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                var isRunningInDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING")?.ToLower() == "true";

                var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = isRunningInDocker ? "db" : dbSettings.Host,
                    Database = dbSettings.Database,
                    Username = dbSettings.Username,
                    Password = dbSettings.Password,
                    Pooling = true,
                    MinPoolSize = 5,
                    MaxPoolSize = 100
                };

                options.UseNpgsql(connectionStringBuilder.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
            });
            
            return services;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add db context: {ex.Message}");
        }
    }
} 