using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using FluentAssertions;
using API.Common.Helpers;
using API.Configuration;
using Microsoft.Extensions.Options;

namespace API.UnitTests;

public abstract class TestBase
{
    protected readonly IServiceCollection _services;
    protected readonly ServiceProvider _serviceProvider;
    protected readonly IConfiguration _configuration;

    protected TestBase()
    {
        // Load configuration from test appsettings.json
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
        
        _services = new ServiceCollection();
        
        // Add test configuration to services
        _services.AddSingleton(_configuration);
        
        // Register configuration using ConfigManager
        ConfigManager.AddConfiguration(_services, _configuration);
        
        // Configure additional test services
        ConfigureTestServices(_services);
        
        _serviceProvider = _services.BuildServiceProvider();
        
        // Initialize helpers
        InitializeHelpers();
    }
    
    private void InitializeHelpers()
    {
        try
        {
            // Initialize helpers with IOptions
            var passwordHasherOptions = _serviceProvider.GetRequiredService<IOptions<PasswordHasherSettings>>();
            var snowflakeOptions = _serviceProvider.GetRequiredService<IOptions<SnowflakeSettings>>();
            
            Hasher.Initialize(passwordHasherOptions);
            Snowflake.Initialize(snowflakeOptions);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize helpers: {ex.Message}", ex);
        }
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Override this in derived classes to configure specific test services
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    protected T GetMock<T>() where T : class
    {
        return Substitute.For<T>();
    }

    protected void RegisterMock<T>(T mock) where T : class
    {
        _services.AddSingleton(mock);
    }

    protected void RegisterService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _services.AddScoped<TService, TImplementation>();
    }
}