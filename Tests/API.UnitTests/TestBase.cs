using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using FluentAssertions;

namespace API.UnitTests;

public abstract class TestBase
{
    protected readonly IServiceCollection _services;
    protected readonly ServiceProvider _serviceProvider;

    protected TestBase()
    {
        _services = new ServiceCollection();
        ConfigureTestServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
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