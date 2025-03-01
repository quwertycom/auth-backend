using API.Infrastructure.Database;
using API.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using API.Infrastructure.Database.Repositories;
using API.Shared.Interfaces.Database.Repositories;

namespace API.Shared.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // First configure DatabaseSettings from configuration
        services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));
        
        // Then override with environment variables if they exist
        services.PostConfigure<DatabaseSettings>(settings => 
        {
            var isDocker = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_RUNNING"));
            
            // Docker environment may use different host names (like 'db' instead of 'localhost')
            settings.Host = (Environment.GetEnvironmentVariable("Postgres__Host") == "db" && !isDocker) ? "localhost" : Environment.GetEnvironmentVariable("Postgres__Host") ?? "localhost";
            settings.Database = Environment.GetEnvironmentVariable("Postgres__Database") ?? "qauth_db";
            settings.Username = Environment.GetEnvironmentVariable("Postgres__User") ?? "postgres";
            settings.Password = Environment.GetEnvironmentVariable("Postgres__Password") ?? "postgres";
        });

        // Register DbContext using the DatabaseSettings
        services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            var connectionString = dbSettings.GetConnectionString();
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddInMemoryDatabaseServices(this IServiceCollection services)
    {
        // For testing purposes
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
            
        // Register repositories
        // services.AddScoped<IUnitOfWork, UnitOfWork>();
        // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        return services;
    }
} 