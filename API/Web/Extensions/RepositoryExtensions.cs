using API.Repositories;
using API.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions;

/// <summary>
/// Extension methods for repository registration
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Adds repositories to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        try
        {
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVerificationRepository, VerificationRepository>();
            
            return services;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add repositories: {ex.Message}");
        }
    }
} 