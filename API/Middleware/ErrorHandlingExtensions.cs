using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public static class ErrorHandlingExtensions
{
    public static IApplicationBuilder ConfigureGlobalErrorHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Status = "INTERNAL_SERVER_ERROR",
                    Message = "An unexpected error occurred. Please try again later. " + exception?.Message
                });
            });
        });

        return app;
    }
} 