using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace API.Middleware;

public static class SwaggerExtensions
{
    public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "api/docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api/docs/v1/swagger.json", "qAuth API V1");
            c.RoutePrefix = "api/docs";
        });

        return app;
    }
} 