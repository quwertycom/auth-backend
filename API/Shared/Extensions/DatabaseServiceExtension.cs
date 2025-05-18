using API.Infrastructure.Database;
using API.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using API.Infrastructure.Database.Repositories;
using API.Shared.Interfaces.Database.Repositories;

namespace API.Shared.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));

        services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            var connectionString = dbSettings.GetConnectionString();

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerificationRepository, VerificationRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        return services;
    }

    public static IServiceCollection AddInMemoryDatabaseServices(this IServiceCollection services)
    {
        // For testing purposes
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        return services;
    }
}