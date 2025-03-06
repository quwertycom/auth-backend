using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using API.Shared.Interfaces.Database;
using API.Shared.Interfaces.Security;
using API.Shared.Configuration;
using API.Shared.Interfaces.Email;
using API.IntegrationTests.Mocks;
using API.Shared.Interfaces.Database.Repositories;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;
using Microsoft.EntityFrameworkCore;
using API.Infrastructure.Database;

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
                    config.AddJsonFile("appsettings.Testing.json", optional: true);
                });

                builder.ConfigureServices(services =>
                {
                    // Completely remove the existing database registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(API.Infrastructure.Database.AuthDbContext));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    var dbOptionsDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<API.Infrastructure.Database.AuthDbContext>));
                    if (dbOptionsDescriptor != null)
                    {
                        services.Remove(dbOptionsDescriptor);
                    }

                    // Also remove the entire database services registration
                    // This is important to prevent multiple registrations
                    var dbServices = services.Where(s => 
                        s.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                        s.ImplementationType?.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true
                    ).ToList();
                    
                    foreach (var service in dbServices)
                    {
                        services.Remove(service);
                    }

                    // Add a clean in-memory database registration
                    services.AddDbContext<API.Infrastructure.Database.AuthDbContext>(options =>
                        options.UseInMemoryDatabase("TestingDb"));

                    // Continue with the rest of the test service configuration
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

        // Configure JwtSettings for testing
        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = "IntegrationTestSecretKey_ThisIsAReallyLongSecretKey";
            options.Issuer = "test-issuer";
            options.Audience = "test-audience";
        });
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

    protected async Task EnsureVerifiedUserExistsAsync(string username, string password)
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        
        // Check if user already exists
        if (!await userRepository.UsernameExistsAsync(username))
        {
            // Create a hash for the password
            var hashedPassword = hasher.Hash(password);
            
            // Create and add a new user with the Active state
            var newUser = new User
            {
                Username = username,
                FirstName = "Test",
                LastName = "User",
                PasswordHash = hashedPassword.hash,
                PasswordSalt = hashedPassword.salt,
                BirthDate = new DateTime(1990, 1, 1),
                Gender = API.Shared.Enums.Entities.User.UserGender.Male,
                State = API.Shared.Enums.Entities.User.UserState.Active // Important: Set as Active, not PendingVerification
            };
            
            await userRepository.AddUserAsync(newUser);
            
            // Add a verified email for the user
            var newEmail = new EmailAddress
            {
                User = newUser,
                Value = $"{username}@example.com",
                State = EmailState.Active, // Important: Set as Verified
                Type = EmailType.Primary
            };
            
            await userRepository.AddEmailAsync(newEmail);
        }
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