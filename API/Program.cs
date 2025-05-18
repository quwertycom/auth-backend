using Microsoft.AspNetCore.Mvc;
using API.Shared.Extensions;
using FastEndpoints;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {

        // Create builder
        var builder = WebApplication.CreateBuilder(args);

        // Load configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Configure services using extension methods
        builder.Services.AddAppConfiguration(builder.Configuration);
        builder.Services.AddDatabaseServices(builder.Configuration);
        builder.Services.AddSecurityServices(builder.Configuration);
        builder.Services.AddEmailServices();

        // Add FastEndpoints
        builder.Services.AddFastEndpoints();

        // Add Swagger/OpenAPI
        builder.Services.AddSwaggerServices();

        // Add application modules
        builder.Services.AddApplicationModules();

        // Add health checks
        builder.Services.AddHealthCheckServices();

        builder.Services.AddProblemDetails();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage ?? "").ToArray() ?? []
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

        app.ConfigurePipeline();

        app.Urls.Add("http://0.0.0.0:8000");

        app.Run();
    }
}
