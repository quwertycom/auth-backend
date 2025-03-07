using API.Features.Session.Revoke.Interfaces;
using API.Features.Session.Revoke.Services;
using API.Features.Session.Refresh.Interfaces;
using API.Features.Session.Refresh.Services;

namespace API.Features.Session;

/// <summary>
/// Session module for the API
/// </summary>
public static class SessionModule
{
    /// <summary>
    /// Add all session services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add the services to</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        services.AddScoped<IRevokeSessionService, RevokeSessionService>();
        services.AddScoped<IRefreshSessionService, RefreshSessionService>();
        
        return services;
    }
}