using Microsoft.OpenApi.Models;
using DotNetEnv;
using API.Data;
using Microsoft.EntityFrameworkCore;

// Load .env file
if (File.Exists("../.env"))
{
    Env.Load("../.env");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connectionString = $"Host=localhost;Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}";
    options.UseNpgsql(connectionString);
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
