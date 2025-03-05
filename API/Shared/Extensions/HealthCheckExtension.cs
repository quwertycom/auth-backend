using API.Shared.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Text.Json;

namespace API.Shared.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // Add the API service health check - always works if the API is running
        healthChecks.AddCheck("self", () => HealthCheckResult.Healthy(),
                tags: ["service"]);

        try
        {
            // Add the database health check with better error handling
            healthChecks.AddNpgSql(
                name: "database",
                connectionStringFactory: sp =>
                {
                    try
                    {
                        var dbSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                        var connectionString = dbSettings.GetConnectionString();

                        // If running locally without Docker, ensure we're using localhost instead of db
                        var isDocker = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_RUNNING"));
                        if (!isDocker && connectionString.Contains("Host=db"))
                        {
                            connectionString = connectionString.Replace("Host=db", "Host=localhost");
                        }

                        return connectionString;
                    }
                    catch (Exception)
                    {
                        return "Host=localhost;Database=postgres;Username=postgres;Password=postgres";
                    }
                },
                tags: ["db", "postgresql"],
                timeout: TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            // Add a dummy failing health check as a placeholder
            healthChecks.AddCheck("database-setup-failed", () =>
                HealthCheckResult.Unhealthy($"Database health check setup failed: {ex.Message}"),
                tags: ["db", "postgresql"]);
        }

        return services;
    }

    public static WebApplication ConfigureHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("service") || check.Tags.Contains("db"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("service"),
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                tags = e.Value.Tags
            })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
    }
}