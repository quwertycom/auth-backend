using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace API.Infrastructure.Database;

/// <summary>
/// Factory for creating DbContext instances during design-time operations like migrations
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Load environment variables from .env file if it exists
        DotNetEnv.Env.Load();
        
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        
        // Get connection string from environment variables
        var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "auth";
        var username = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "postgres";
        
        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        
        optionsBuilder
            .UseNpgsql(connectionString, options => {
                options.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                options.MigrationsAssembly("API");
            });
        
        return new AuthDbContext(optionsBuilder.Options);
    }
} 