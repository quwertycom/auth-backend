using Microsoft.Extensions.DependencyInjection;

namespace API.Infrastructure.Extensions;

/// <summary>
/// Extension methods for database-related service registration
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds database context to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        // Skip in testing environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Testing") 
            return services;
            
        // Use the dedicated DbContext extension
        return services.AddAuthDbContext();
    }
} 