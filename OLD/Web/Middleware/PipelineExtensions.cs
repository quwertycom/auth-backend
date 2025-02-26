using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Middleware;

public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure error handling based on environment
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
        
        // Enable Swagger in all environments
        app.UseSwaggerServices();
        
        app.UseHttpsRedirection();
        
        // Ensure static files are served (needed for health check UI CSS)
        app.UseStaticFiles();
        
        app.UseRouting();
        app.UseCors();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Add rate limiting middleware
        app.UseRateLimiter();
        
        // Add security headers
        app.Use((context, next) => 
        {
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            return next();
        });
        
        app.MapControllers();
        
        // Configure all health check endpoints
        app.ConfigureHealthChecks();
        
        return app;
    }
} 