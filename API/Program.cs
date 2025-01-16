using Microsoft.OpenApi.Models;
using API.Data;
using API.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        // Enable legacy timestamp behavior for Npgsql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // Log the environment
        Console.WriteLine($"Current environment: {builder.Environment.EnvironmentName}");

        // Load configuration based on the environment
        IConfiguration configuration;
        if (builder.Environment.IsProduction())
        {
            Console.WriteLine("Loading production configuration...");
            configuration = ConfigManager.LoadProductionConfig();
        }
        else
        {
            Console.WriteLine("Loading development configuration...");
            configuration = ConfigManager.LoadDevelopmentConfig();
        }

        // Log configuration values for debugging
        Console.WriteLine("\nConfiguration values:");
        Console.WriteLine($"JWT__SecretKey length: {(configuration["JWT__SecretKey"]?.Length ?? 0)} chars");
        Console.WriteLine($"JWT__Issuer: {configuration["JWT__Issuer"]}");
        Console.WriteLine($"JWT__Audience: {configuration["JWT__Audience"]}");
        Console.WriteLine($"Email__Host: {configuration["Email__Host"]}");
        Console.WriteLine($"POSTGRES_DB: {configuration["POSTGRES_DB"]}");
        Console.WriteLine($"DOCKER_RUNNING: {configuration["DOCKER_RUNNING"]}\n");

        // Log some configuration values to verify loading
        try
        {
            Console.WriteLine($"JWT:Issuer = {configuration["JWT:Issuer"]}");
            Console.WriteLine($"Email:Host = {configuration["Email:Host"]}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading configuration: {ex.Message}");
        }

        // Add the loaded configuration to the builder
        builder.Configuration.AddConfiguration(configuration);

        // Configure Kestrel
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.AddServerHeader = false;
            serverOptions.AllowSynchronousIO = false;
            serverOptions.ConfigureEndpointDefaults(listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
            });
            serverOptions.ListenAnyIP(8000); // Listen on port 8000
        });

        // Set URLs
        builder.WebHost.UseUrls("http://0.0.0.0:8000");

        // Initialize services via Services helper
        Services.Initialize(builder);

        // Add services to the container
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = false; // Enable ProblemDetails for client errors
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails(); // Add ProblemDetails service

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
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

        // Configure HTTPS
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = null;
            });
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Always enable Swagger in development and Docker
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "api/docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api/docs/v1/swagger.json", "qAuth API V1");
            c.RoutePrefix = "api/docs";
        });

        // Use CORS before routing
        app.UseCors();

        // Add routing and other middleware
        app.UseRouting();

        // Add authentication middleware before authorization
        app.UseAuthentication();
        app.UseAuthorization();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Map controllers and health checks
        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
