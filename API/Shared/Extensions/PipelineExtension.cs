using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Shared.Extensions;

public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure environment-specific settings
        app.ConfigureEnvironment();

        // Configure core middleware components
        app.UseSwaggerServices();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        
        // Configure security middleware
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.UseSecurityHeaders();
        
        // Map endpoints
        app.MapControllers();
        app.ConfigureHealthChecks();
        
        return app;
    }

    private static WebApplication ConfigureEnvironment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("DockerDevelopment"))
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(exceptionHandlerApp => 
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(new ProblemDetails {
                        Title = "An error occurred",
                        Detail = "See logs for details",
                        Status = 500
                    });
                });
            });
            app.UseHsts();
        }
        return app;
    }

    private static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use((context, next) => 
        {
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            return next();
        });
        
        return app;
    }
} 