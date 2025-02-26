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
        
        // Check if running in Docker by environment variable
        var isDocker = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_RUNNING"));
        
        // Output environment diagnostic information
        Console.WriteLine($"Current environment: {builder.Environment.EnvironmentName}");
        Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
        Console.WriteLine($"DOCKER_RUNNING: {Environment.GetEnvironmentVariable("DOCKER_RUNNING")}");
        Console.WriteLine($"IsDocker detected: {isDocker}");
        
        // If Docker environment variable is set but environment isn't properly set, force it
        if (isDocker && !builder.Environment.IsEnvironment("DockerDevelopment") && !builder.Environment.IsProduction())
        {
            Console.WriteLine("Forcing environment to DockerDevelopment based on DOCKER_RUNNING variable");
            builder.Environment.EnvironmentName = "DockerDevelopment";
        }

        // Verify the environment after any potential changes
        Console.WriteLine($"Final environment: {builder.Environment.EnvironmentName}");

        // Add all application services using extension methods
        builder.Services.AddApplicationServices(builder.Configuration);
        
        builder.Services.ConfigureCors();

        // Environment-specific service configuration
        if (builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("DockerDevelopment"))
        {
            ConfigureDevelopmentServices(builder.Services);
        }
        else if (builder.Environment.IsEnvironment("DockerDevelopment"))
        {
            ConfigureDockerDevelopmentServices(builder.Services);
        }
        else
        {
            ConfigureProductionServices(builder.Services);
        }

        // Common services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<IEnvironmentVariableProvider, EnvironmentVariableProvider>();
        
        // Configure health checks
        builder.Services.AddHealthChecks()
            .AddCheck("Environment", () => ConfigManager.ValidateEnvironmentVariables(builder.Configuration))
            .AddCheck<API.Web.HealthChecks.PostgreSqlHealthCheck>("PostgreSQL", tags: new[] { "database", "postgresql", "ready" })
            .AddCheck<API.Web.HealthChecks.SmtpHealthCheck>("SMTP", tags: new[] { "email", "smtp", "ready" })
            .AddCheck<API.Web.HealthChecks.DockerNetworkHealthCheck>("DockerNetwork", tags: new[] { "network", "docker", "ready" })
            .AddCheck<API.Web.HealthChecks.MailHogHealthCheck>("MailHog", tags: new[] { "email", "mailhog", "ready" });
        
        // Add health checks UI
        builder.Services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(60); // Evaluate status every 60 seconds
            options.MaximumHistoryEntriesPerEndpoint(50); // Keep 50 entries in history
            options.SetApiMaxActiveRequests(1); // Prevent multiple requests
            
            // For Docker, the API service will be accessible at http://api:8000
            // For local development, use localhost
            var apiUrl = builder.Environment.IsEnvironment("DockerDevelopment")
                ? "http://api:8000/health"
                : "http://localhost:8000/health";
                
            options.AddHealthCheckEndpoint("API", apiUrl);
        })
        .AddInMemoryStorage(); // Store health check result in memory
            
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
        if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("DockerDevelopment"))
        {
            builder.Services.AddHsts(options => 
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });
        }

        if (builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("DockerDevelopment"))
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

        // Map health checks UI endpoint
        app.MapHealthChecksUI(options => 
        {
            options.UIPath = "/health-ui"; // Health check dashboard UI at /health-ui
            options.ApiPath = "/health-api";
            options.AddCustomStylesheet("wwwroot/css/healthchecks-custom.css");
        });

        // Get API port from configuration using IOptions pattern
        var apiSettings = app.Services.GetRequiredService<IOptions<ApiSettings>>().Value;
        
        // When running in Docker or Production, bind to all interfaces using 0.0.0.0
        // When running locally, use localhost for better security
        string bindAddress = app.Environment.IsEnvironment("DockerDevelopment") || !app.Environment.IsDevelopment()
            ? "0.0.0.0"  // Docker or Production
            : "localhost"; // Local development
        app.Urls.Add($"http://{bindAddress}:{apiSettings.Port}");
        
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

    private static void ConfigureDockerDevelopmentServices(IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddSingleton<IDeveloperEmailService, LocalEmailService>();
        // Add any Docker-specific development services here
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
