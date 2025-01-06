using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using API;

namespace API.UnitTests;

public abstract class TestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;

    protected TestBase()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure test services
                    ConfigureTestServices(services);
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Override this in derived classes to configure specific test services
    }

    public void Dispose()
    {
        _scope.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }
} 