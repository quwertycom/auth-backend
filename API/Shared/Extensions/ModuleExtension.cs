using API.Features.Authentication;

namespace API.Shared.Extensions;

/// <summary>
/// Extension methods for adding feature modules to the service collection.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registers all feature modules.
    /// </summary>
    /// <param name="services">The service collection to add the modules to.</param>
    /// <returns>The service collection with the modules added.</returns>
    public static IServiceCollection AddApplicationModules(this IServiceCollection services)
    {
        services.AddAuthenticationServices();

        return services;
    }
}