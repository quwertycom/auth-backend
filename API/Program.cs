using Microsoft.OpenApi.Models;
using API.Data;
using API.Common.Helpers;
using API.Configuration;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        // Enable legacy timestamp behavior for Npgsql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // Configure strongly-typed settings
        builder.Services.Configure<DatabaseSettings>(
            builder.Configuration.GetSection("Database"));
        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("Jwt"));

        // Initialize services via Services helper
        Services.Initialize(builder);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();

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

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

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

        // Use CORS before routing
        app.UseCors();

        // Add routing and other middleware
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Map endpoints
        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
