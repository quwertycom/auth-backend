
namespace API.Shared.Interfaces.Configuration;

/// <summary>
/// Interface for managing configuration settings.
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Gets the configuration for the given environment.
    /// </summary>
    IConfiguration GetConfiguration(bool isDevelopment);

    /// <summary>
    /// Adds the configuration to the service collection.
    /// </summary>
    void AddConfiguration(IServiceCollection services, IConfiguration configuration);
}