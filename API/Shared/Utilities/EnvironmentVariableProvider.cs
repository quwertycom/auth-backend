using Microsoft.Extensions.Configuration;

namespace API.Shared.Utilities;

public interface IEnvironmentVariableProvider
{
    string? GetVariable(string name);
    bool IsProduction { get; }
}

public class EnvironmentVariableProvider : IEnvironmentVariableProvider
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public EnvironmentVariableProvider(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public string? GetVariable(string name)
    {
        return _configuration[name] ?? Environment.GetEnvironmentVariable(name);
    }

    public bool IsProduction => _environment.IsProduction();
} 