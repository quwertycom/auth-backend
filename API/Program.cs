using Microsoft.AspNetCore.Mvc;
using API.Shared.Extensions;
using FastEndpoints;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        // Enable legacy timestamp behavior for Npgsql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Create builder
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure environment variables and configuration sources with proper priority
        builder.Configuration
            .AddDotEnvConfiguration(builder.Environment)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Configure services using extension methods
        builder.Services.AddAppConfiguration(builder.Configuration);
        builder.Services.AddDatabaseServices(builder.Configuration);
        builder.Services.AddSecurityServices(builder.Configuration);
        builder.Services.AddEmailServices(builder.Configuration);

        // Add FastEndpoints
        builder.Services.AddFastEndpoints();

        // Add Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerServices();

        // Add application modules
        builder.Services.AddApplicationModules();

        // Add health checks
        builder.Services.AddHealthCheckServices(builder.Configuration);

        builder.Services.AddProblemDetails();

        // Configure CORS - (Note: This is already in SecurityServiceExtensions)
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

        var app = builder.Build();

        // Configure the HTTP request pipeline using extension methods
        app.ConfigurePipeline();

        // Explicitly bind to all interfaces
        app.Urls.Add("http://0.0.0.0:8000");

        app.Run();
    }
}