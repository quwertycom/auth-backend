using Microsoft.OpenApi.Models;
using API.Data;
using API.Common.Helpers;
using API.Configuration;
using API.Common.Utilities.Interfaces;
using API.Common.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using API.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using API.Extensions;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        // Enable legacy timestamp behavior for Npgsql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // Add all application services using extension methods
        builder.Services.AddApplicationServices(builder.Configuration);
        
        builder.Services.ConfigureCors();

        // Environment-specific service configuration
        if (builder.Environment.IsDevelopment())
        {
            ConfigureDevelopmentServices(builder.Services);
        }
        else
        {
            ConfigureProductionServices(builder.Services);
        }

        // Common services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<IEnvironmentVariableProvider, EnvironmentVariableProvider>();
        builder.Services.AddHealthChecks()
            .AddCheck("Environment", () => ConfigManager.ValidateEnvironmentVariables(builder.Configuration));
        builder.Services.AddProblemDetails();
        
        // Configure rate limiting with settings from config
        ConfigureRateLimiting(builder.Services);

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
                    Status = "INVALID_REQUEST",
                    Message = "Invalid request format: " + string.Join(", ", errors.Values.SelectMany(v => v)),
                    Errors = errors
                });
            };
        });

        // Replace manual environment checks with ASP.NET Core conventions
        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHsts(options => 
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });
        }

        // Environment-specific service configuration
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IDeveloperTools, DeveloperTools>();
        }
        else if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddScoped<IMockServices, TestingMockServices>();
        }

        var app = builder.Build();

        // Configure pipeline based on environment
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "qAuth API v1"));
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

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseRateLimiter();

        app.Use((context, next) => 
        {
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            return next();
        });

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Get API port from configuration using IOptions pattern
        var apiSettings = app.Services.GetRequiredService<IOptions<ApiSettings>>().Value;
        app.Urls.Add($"http://0.0.0.0:{apiSettings.Port}");
        
        app.Run();
    }

    private static void ConfigureDevelopmentServices(IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddSingleton<IDeveloperEmailService, LocalEmailService>();
    }

    private static void ConfigureProductionServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddScoped<IEmailService, SendGridEmailService>();
    }
    
    private static void ConfigureRateLimiting(IServiceCollection services)
    {
        // Use a factory pattern to inject IOptions in a service collection compatible way
        services.AddRateLimiter(options => 
        {
            options.RejectionStatusCode = 429;
            
            // Get rate limiting settings once at startup
            using var serviceProvider = services.BuildServiceProvider();
            var rateLimitingSettings = serviceProvider.GetRequiredService<IOptions<RateLimitingSettings>>().Value;
            
            options.AddPolicy("jwt-auth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    context.User.Identity?.Name ?? "anonymous",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(rateLimitingSettings.WindowInMinutes),
                        PermitLimit = rateLimitingSettings.PermitLimit,
                        SegmentsPerWindow = rateLimitingSettings.SegmentsPerWindow
                    })
            );
        });
    }
}
