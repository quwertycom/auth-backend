
namespace API.Common.Utilities.Interfaces;

public interface IConfigManager
{
    IConfiguration GetConfiguration(bool isDevelopment);
    void AddConfiguration(IServiceCollection services, IConfiguration configuration);
}