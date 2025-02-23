using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using API.Common.Helpers;
using Microsoft.AspNetCore.Hosting;
using API.Services.Interfaces;
using API.IntegrationTests.Mocks;
using API.Services;
using API.Repositories.Interfaces;
using API.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using API.Common.Utilities.Interfaces;

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
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) => 
                {
                    config.AddInMemoryCollection([
                        new KeyValuePair<string, string?>("ASPNETCORE_ENVIRONMENT", "Testing"),
                        new KeyValuePair<string, string?>("DOCKER_RUNNING", "false")
                    ]);
                });
                builder.ConfigureServices(services =>
                {
                    ConfigManager.AddConfiguration(services);
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
        // 1. Configure test environment variables (Keep these)
        Environment.SetEnvironmentVariable("POSTGRES_DB", "test_db");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "test_user");

        ConfigManager.AddConfiguration(services); // Keep this line to load other configurations

        // 2. Override database context configuration to use InMemoryDatabase
        services.RemoveAll<AuthDbContext>(); // Use RemoveAll<AuthDbContext>() instead
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase("IntegrationTestDb"));

        // Add controller services (Keep these)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ISessionService, SessionService>();

        // Add repositories (Keep these)
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerificationRepository, VerificationRepository>();

        // Mock services
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

    protected async Task ResetDatabase()
    {
        var context = GetRequiredService<AuthDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
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