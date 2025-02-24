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

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        // Log "Hello World" to the console
        Console.WriteLine("Hello World");

        // Enable legacy timestamp behavior for Npgsql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // Add services via ServiceInitializer helper
        var services = new Common.Utilities.Services(builder);
        services.Initialize();
        
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
        builder.Services.AddRateLimiter(options => 
        {
            options.RejectionStatusCode = 429;
            options.AddPolicy("jwt-auth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    context.User.Identity?.Name ?? "anonymous",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 20,
                        SegmentsPerWindow = 4
                    }));
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

        // Removed environment check here - now handled in pipeline
        app.Urls.Add("http://0.0.0.0:8000");
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
}
