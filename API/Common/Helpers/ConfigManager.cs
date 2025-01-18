using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace API.Common.Helpers;

public static class ConfigManager
{
    private static IConfiguration? _configuration;
    
    public static IConfiguration GetConfiguration(bool isDevelopment)
    {
        if (_configuration != null)
            return _configuration;

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{(isDevelopment ? "Development" : "Production")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true);

        _configuration = builder.Build();
        return _configuration;
    }
}