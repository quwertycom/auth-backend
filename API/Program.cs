using API.Common.Helpers;
using API.Common.Utilities.Interfaces;
using API.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using API.Infrastructure.Extensions;
using API.Web.Middleware;
using API.Web.Configuration;

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

        // Configure Swagger using extension method
        builder.Services.AddSwaggerServices();

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

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IDeveloperTools, DeveloperTools>();
        }
        else if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddScoped<IMockServices, TestingMockServices>();
        }

        var app = builder.Build();

        // Configure the middleware pipeline using the extension method
        app = app.ConfigurePipeline();

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
