using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Email;
using API.Shared.Configuration;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;

namespace API.Tests.Functional;

public abstract class TestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;

    protected TestBase()
    {
        // Setup environment for testing
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
                    // Remove the existing database context
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

                    // Remove any existing EF Core registrations
                    var dbServices = services.Where(s =>
                        s.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                        s.ImplementationType?.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true
                    ).ToList();

                    foreach (var service in dbServices)
                    {
                        services.Remove(service);
                    }

                    // Add in-memory database for testing
                    var databaseName = $"FunctionalTestDb_{Guid.NewGuid()}";
                    services.AddDbContext<API.Infrastructure.Database.AuthDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName));

                    // Configure other test services
                    ConfigureTestServices(services);
                });
            });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _scope = _factory.Services.CreateScope();
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<T>();
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Mock email service for testing to prevent actual emails
        // Find and remove the existing email sender registration
        var emailSenderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
        if (emailSenderDescriptor != null)
        {
            services.Remove(emailSenderDescriptor);
        }
        services.AddScoped<IEmailSender, MockEmailSender>();
        
        // Configure JWT settings for testing
        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = "FunctionalTestSecretKey_ThisIsAReallyLongSecretKeyForTesting";
            options.Issuer = "test-issuer";
            options.Audience = "test-audience";
        });
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
            var newUser = new API.Infrastructure.Database.Entities.User.User
            {
                Username = username,
                FirstName = "Test",
                LastName = "User",
                PasswordHash = hashedPassword.Hash,
                PasswordSalt = hashedPassword.Salt,
                BirthDate = new DateTime(1990, 1, 1),
                Gender = API.Shared.Enums.Entities.User.UserGender.Male,
                State = API.Shared.Enums.Entities.User.UserState.Active // Important: Set as Active, not PendingVerification
            };

            await userRepository.AddUserAsync(newUser);

            // Add a verified email for the user
            var newEmail = new API.Infrastructure.Database.Entities.User.EmailAddress
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

// Mock email sender for testing
public class MockEmailSender : IEmailSender
{
    public readonly List<(string To, string Subject, string Content)> SentEmails = new();
    public readonly List<(string ToEmail, string Otp, string FirstName, string Language)> SentOtpEmails = new();
    public readonly List<(string ToEmail, string Code, string Language)> SentResetPasswordEmails = new();

    public Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        // Log instead of sending in tests
        Console.WriteLine($"Mock email sent to: {to}, Subject: {subject}");
        SentEmails.Add((to, subject, htmlContent));
        return Task.CompletedTask;
    }
    
    public Task<bool> SendOtpEmailAsync(string toEmail, string otp, string firstName, string language = "en")
    {
        Console.WriteLine($"Mock OTP email sent to: {toEmail}, Code: {otp}, FirstName: {firstName}, Language: {language}");
        SentOtpEmails.Add((toEmail, otp, firstName, language));
        return Task.FromResult(true);
    }
    
    public Task<bool> SendResetPasswordEmailAsync(string toEmail, string code, string language = "en")
    {
        Console.WriteLine($"Mock Reset Password email sent to: {toEmail}, Code: {code}, Language: {language}");
        SentResetPasswordEmails.Add((toEmail, code, language));
        return Task.FromResult(true);
    }
}