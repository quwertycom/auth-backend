using Microsoft.AspNetCore.Builder;

namespace API.Middleware;

public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.ConfigureGlobalErrorHandler();
        
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Development-only components
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerServices();
        }
        
        app.MapControllers();
        app.ConfigureHealthChecks();
        
        return app;
    }
} 