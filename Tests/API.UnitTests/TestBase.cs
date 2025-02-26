using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using FluentAssertions;
using API.Common.Helpers;
using API.Web.Configuration;
using API.Infrastructure.Services.HostedServices;
using Microsoft.Extensions.Hosting;
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
        
        // Add helper initialization hosted services
        _services.AddHostedService<HasherInitializationService>();
        _services.AddHostedService<SnowflakeInitializationService>();
        
        // Configure additional test services
        ConfigureTestServices(_services);
        
        _serviceProvider = _services.BuildServiceProvider();
        
        // Start the hosted services to initialize helpers
        InitializeHostedServices().GetAwaiter().GetResult();
    }
    
    private async Task InitializeHostedServices()
    {
        try
        {
            // Get and start all hosted services
            var hostedServices = _serviceProvider.GetServices<IHostedService>();
            foreach (var service in hostedServices)
            {
                await service.StartAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize hosted services: {ex.Message}", ex);
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