using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Json;
using API;

namespace API.IntegrationTests;

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
                    // Configure services for integration testing
                    // This will use the real database from test settings
                    ConfigureTestServices(services);
                });
            });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        });

        _scope = _factory.Services.CreateScope();
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Override this in derived classes to configure specific test services
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        return await _client.GetAsync(endpoint);
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T content)
    {
        return await _client.PostAsJsonAsync(endpoint, content);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T content)
    {
        return await _client.PutAsJsonAsync(endpoint, content);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await _client.DeleteAsync(endpoint);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }
}