using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace API.Middleware;

public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder ConfigureHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");
        return endpoints;
    }
} 