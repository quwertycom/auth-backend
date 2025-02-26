using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using API.Common.Helpers;
using Microsoft.AspNetCore.Hosting;
using API.Core.Services.Interfaces;
using API.IntegrationTests.Mocks;
using API.Infrastructure.Services;
using API.Infrastructure.Repositories.Interfaces;
using API.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using API.Common.Utilities.Interfaces;
using Microsoft.Extensions.Options;
using API.Web.Configuration;

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
        // 1. Remove existing options registrations
        services.RemoveAll<IOptions<DatabaseSettings>>();
        services.RemoveAll<IOptionsMonitor<DatabaseSettings>>();
        services.RemoveAll<IOptionsSnapshot<DatabaseSettings>>();
        services.RemoveAll<IOptionsFactory<DatabaseSettings>>();

        // 2. Configure all required settings
        // Database settings
        services.AddOptions<DatabaseSettings>()
            .Configure(settings => {
                settings.Host = "test-host";
                settings.Database = "test-db";
                settings.Username = "test-user";
                settings.Password = "test-password";
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // JWT settings
        services.AddOptions<API.Web.Configuration.JwtSettings>()
            .Configure(settings => {
                settings.SecretKey = "testing-secret-key-that-is-at-least-32-chars-long";
                settings.Issuer = "test-issuer";
                settings.Audience = "test-audience";
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Email settings
        services.AddOptions<EmailSettings>()
            .Configure(settings => {
                settings.Host = "test-smtp";
                settings.Port = 25;
                settings.Username = "test@example.com";
                settings.Password = "test-password";
                settings.FromEmail = "test@example.com";
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        // API settings
        services.AddOptions<ApiSettings>()
            .Configure(settings => {
                settings.Port = "5000";
                settings.FrontendBaseUrl = "http://localhost:3000";
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        // PasswordHasher settings
        services.AddOptions<PasswordHasherSettings>()
            .Configure(settings => {
                settings.Iterations = 1000; // Lower for tests
                settings.SaltSize = 16;
                settings.KeySize = 32;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        // Snowflake settings
        services.AddOptions<SnowflakeSettings>()
            .Configure(settings => {
                settings.DatacenterId = 1;
                settings.WorkerId = 1;
                settings.Epoch = "2024-01-01T00:00:00Z";
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 3. Override database context configuration to use InMemoryDatabase
        services.RemoveAll<AuthDbContext>();
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