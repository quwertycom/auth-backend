using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using HealthChecks.UI.Client;

namespace API.Web.Middleware;

public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder ConfigureHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        return endpoints;
    }
} 