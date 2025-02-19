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

        // Get database configuration
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ??
                      throw new InvalidOperationException("POSTGRES_DB not configured");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ??
                      throw new InvalidOperationException("POSTGRES_USER not configured");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ??
                      throw new InvalidOperationException("POSTGRES_PASSWORD not configured");

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

        optionsBuilder.UseNpgsql(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}