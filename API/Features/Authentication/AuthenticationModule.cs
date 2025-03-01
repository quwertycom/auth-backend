using API.Features.Authentication.Register.Interfaces;
using API.Features.Authentication.Register.Services;

namespace API.Features.Authentication;

/// <summary>
/// Authentication module for the API
/// </summary>
public static class AuthenticationModule
{
    /// <summary>
    /// Add all authentication services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add the services to</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<IRegisterService, RegisterService>();

        return services;
    }
}