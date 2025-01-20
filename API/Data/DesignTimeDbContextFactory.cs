using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings.json and environment variables
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Try to load .env file if it exists (for development)
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
        if (File.Exists(envFile))
        {
            foreach (var line in File.ReadAllLines(envFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }

        // Get database configuration with fallbacks
        var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? 
                   Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? 
                   configuration.GetValue<string>("Database:Host") ?? 
                   "localhost";

        var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? 
                   Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? 
                   configuration.GetValue<string>("Database:Port") ?? 
                   "5432";

        var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? 
                      Environment.GetEnvironmentVariable("POSTGRES_DB") ?? 
                      throw new InvalidOperationException("Database name not configured. Set DATABASE_NAME or POSTGRES_DB environment variable.");

        var username = Environment.GetEnvironmentVariable("DATABASE_USER") ?? 
                      Environment.GetEnvironmentVariable("POSTGRES_USER") ?? 
                      throw new InvalidOperationException("Database username not configured. Set DATABASE_USER or POSTGRES_USER environment variable.");

        var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? 
                      Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? 
                      throw new InvalidOperationException("Database password not configured. Set DATABASE_PASSWORD or POSTGRES_PASSWORD environment variable.");

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
} 