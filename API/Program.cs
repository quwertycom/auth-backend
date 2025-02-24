using Microsoft.OpenApi.Models;
using API.Data;
using API.Common.Helpers;
using API.Configuration;
using API.Common.Utilities.Interfaces;
using API.Common.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using API.Middleware;

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
                    Status = "INVALID_REQUEST",
                    Message = "Invalid request format: " + string.Join(", ", errors.Values.SelectMany(v => v)),
                    Errors = errors
                });
            };
        });

        var app = builder.Build();

        // Consolidated middleware pipeline
        app.ConfigurePipeline();

        // Environment-specific configuration remains here
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.Urls.Add("http://0.0.0.0:8000");
        app.Run();
    }
}
