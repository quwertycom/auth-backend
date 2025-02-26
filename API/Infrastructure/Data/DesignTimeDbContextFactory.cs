using API.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace API.Data;

/// <summary>
/// Design-time factory for EF Core migrations
/// This class is only used by EF Core tools for migrations
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Set up configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Determine if running in Docker
        bool isDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING")?.ToLower() == "true";
            
        // Create DB context options
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            
        // Get database settings
        var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
            
        if (dbSettings == null)
        {
            // Fallback connection string if configuration is missing
            optionsBuilder.UseNpgsql("Host=localhost;Database=qauth_db;Username=qauth_user;Password=qauth_password");
        }
        else
        {
            // Use host from environment or config
            string host = isDocker ? "db" : dbSettings.Host ?? "localhost";

            // Build connection string with settings from configuration
            var connectionString = $"Host={host};Database={dbSettings.Database};Username={dbSettings.Username};Password={dbSettings.Password}";
            optionsBuilder.UseNpgsql(connectionString);
        }
            
        return new AuthDbContext(optionsBuilder.Options);
    }
}