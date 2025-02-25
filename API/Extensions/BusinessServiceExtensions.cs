using API.Common.Helpers;
using API.Services;
using API.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions;

/// <summary>
/// Extension methods for business service registration
/// </summary>
public static class BusinessServiceExtensions
{
    /// <summary>
    /// Adds business services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        try
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<JwtService>();
            
            return services;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add business services: {ex.Message}");
        }
    }
} 