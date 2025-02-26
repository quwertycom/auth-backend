using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text;
using HealthChecks.UI.Client;

namespace API.Web.Middleware;

public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder ConfigureHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Basic health check endpoint for Kubernetes/load balancers
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Liveness probe - just confirms the app is responsive
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // No checks, just returns 200 OK if app is running
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Detailed health check for all services
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            AllowCachingResponses = false
        });

        // Database-specific health check
        endpoints.MapHealthChecks("/health/database", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Email service health check
        endpoints.MapHealthChecks("/health/email", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("email"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // MailHog specific health check
        endpoints.MapHealthChecks("/health/mailhog", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("mailhog"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // Docker network health check
        endpoints.MapHealthChecks("/health/network", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("network"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return endpoints;
    }
} 