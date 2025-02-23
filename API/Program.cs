using Microsoft.OpenApi.Models;
using API.Data;
using API.Common.Helpers;
using API.Configuration;
using API.Common.Utilities.Interfaces;
using API.Common.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        // Enable legacy timestamp behavior for Npgsql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // Configure strongly-typed settings
        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection("Email"));
        // Add services via ServiceInitializer helper
        var services = new Common.Utilities.Services(builder);
        services.Initialize();
        

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();

        // Configure CORS - Update with specific origins in production
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowed(_ => true) // Be careful with this in production
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Configure Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "qAuth API",
                Version = "v1",
                Description = "An ASP.NET Core Web API"
            });
        });

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage ?? "").ToArray() ?? Array.Empty<string>()
                    );

                return new BadRequestObjectResult(new
                {
                    Status = "BAD_REQUEST",
                    Message = "Invalid request format",
                    Errors = errors
                });
            };
        });

        var app = builder.Build();

        // Add global error handling middleware FIRST in pipeline
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                // Log error here if needed (requires ILogger injection)
                
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Status = "INTERNAL_SERVER_ERROR",
                    Message = "An unexpected error occurred. Please try again later. " + exception?.Message
                });
            });
        });

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Use CORS and other middleware in correct order
        app.UseRouting();
        app.UseCors();

        // Configure Swagger
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "api/docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api/docs/v1/swagger.json", "qAuth API V1");
            c.RoutePrefix = "api/docs";
        });

        // Add authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure HTTPS redirection conditionally
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Map endpoints
        app.MapControllers();
        app.MapHealthChecks("/health");

        // Explicitly bind to all interfaces
        app.Urls.Add("http://0.0.0.0:8000");

        app.Run();
    }
}
