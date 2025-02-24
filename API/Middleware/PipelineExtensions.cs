using Microsoft.AspNetCore.Builder;

namespace API.Middleware;

public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.ConfigureGlobalErrorHandler();
        app.UseRouting();
        app.UseCors();
        app.ConfigureSwagger();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.ConfigureHealthChecks();
        
        return app;
    }
} 