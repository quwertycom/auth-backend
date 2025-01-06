using Microsoft.OpenApi.Models;
using DotNetEnv;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "qAuth API V1");
        c.RoutePrefix = "api/docs";
    });
}

app.UseCors();

app.UseRouting();

app.MapControllers();

app.Run();
