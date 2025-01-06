using Microsoft.OpenApi.Models;
using DotNetEnv;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.AddServerHeader = false;
            serverOptions.AllowSynchronousIO = false;
            serverOptions.ConfigureEndpointDefaults(listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
            });
        });

        // Load .env file before configuration setup
        if (File.Exists("../.env"))
        {
            Env.Load("../.env");
        }

        // Add configuration sources
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddEnvironmentVariables();

        // Configure database connection
        builder.Services.AddDbContext<AuthDbContext>(options =>
        {
            var isRunningInDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING")?.ToLower() == "true";
            var host = isRunningInDocker ? "db" : "localhost";

            var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = host,
                Database = builder.Configuration["POSTGRES_DB"],
                Username = builder.Configuration["POSTGRES_USER"],
                Password = builder.Configuration["POSTGRES_PASSWORD"],
                Pooling = true,
                MinPoolSize = 5,
                MaxPoolSize = 100
            };

            options.UseNpgsql(connectionStringBuilder.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
            });
        });

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
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "qAuth API V1");
                c.RoutePrefix = "api/docs";
            });
        }
        else
        {
            // Add error handling for production
            app.UseExceptionHandler();
            app.UseHsts();
            // Enable HTTPS Redirection only in production
            app.UseHttpsRedirection();
        }

        // Use CORS before routing
        app.UseCors();

        // Remove the HTTPS redirection from here since we only want it in production
        // app.UseHttpsRedirection();

        // Add status code pages
        app.UseStatusCodePages();

        // Use routing and authorization middleware
        app.UseRouting();
        app.UseAuthorization();

        // Map controllers and health checks
        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
