using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace API.Infrastructure.Database;

/// <summary>
/// Factory for creating DbContext instances during design-time operations like migrations
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Load environment variables from .env file with absolute path
        var currentDirectory = Directory.GetCurrentDirectory();
        var rootDirectory = currentDirectory.EndsWith("API")
            ? Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory
            : currentDirectory;
        var envPath = Path.Combine(rootDirectory, ".env");

        Console.WriteLine($"Loading .env from: {envPath}");
        DotNetEnv.Env.Load(envPath);

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Get connection s tring from environment variables
        var host = Environment.GetEnvironmentVariable("Postgres__Host") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("Postgres__Port") ?? "5432";
        var database = Environment.GetEnvironmentVariable("Postgres__Database") ?? "auth";
        var username = Environment.GetEnvironmentVariable("Postgres__User") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("Postgres__Password") ?? "postgres";

        Console.WriteLine($"Database connection: Host={host}, Database={database}, User={username}");

        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

        optionsBuilder
            .UseNpgsql(connectionString, options =>
            {
                options.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                options.MigrationsAssembly("API");
            });

        return new AuthDbContext(optionsBuilder.Options);
    }
}