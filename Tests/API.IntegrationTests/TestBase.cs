using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;

using API.Shared.Configuration;
using API.Shared.Interfaces.Email;
using API.IntegrationTests.Mocks;

namespace API.IntegrationTests;

public abstract class TestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;

    protected TestBase()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOCKER_RUNNING", "false");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Use appsettings.Testing.json for tests
                    config.AddJsonFile("appsettings.Testing.json", optional: false);
                });

                builder.ConfigureServices(services =>
                {
                    ConfigureTestServices(services);
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Use mock email sender rather than real one
        services.RemoveAll<IEmailSender>();
        services.AddScoped<IEmailSender, MockEmailSender>();
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
        // Clean up environment variables
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOCKER_RUNNING", null);
        _scope.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }
}