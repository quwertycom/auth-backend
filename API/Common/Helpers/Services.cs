using API.Data;
using API.Service;
using API.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace API.Common.Helpers;

public static class Services
{
	public static void Initialize(WebApplicationBuilder builder)
	{
		// Add configuration first
		ConfigManager.AddConfiguration(builder.Services, builder.Configuration);

		// Add other services
		AddDbContext(builder);
		AddControllerServices(builder);
		InitializeHelpers(builder.Configuration);
	}

	private static void AddControllerServices(WebApplicationBuilder builder)
	{
		try
		{
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<ISessionRepository, SessionRepository>();
			builder.Services.AddScoped<ITokenRepository, TokenRepository>();
			builder.Services.AddScoped<IUserInfoRepository, UserInfoRepository>();
			// add other services in the future
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to add controller services: {ex.Message}");
		}
	}

	private static void AddDbContext(WebApplicationBuilder builder)
	{
		try
		{
			builder.Services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
			{
				var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
				var isRunningInDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING")?.ToLower() == "true";

				var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
				{
					Host = isRunningInDocker ? "db" : dbSettings.Host,
					Database = dbSettings.Database,
					Username = dbSettings.Username,
					Password = dbSettings.Password,
					Pooling = true,
					MinPoolSize = 5,
					MaxPoolSize = 100
				};

				options.UseNpgsql(connectionStringBuilder.ConnectionString, npgsqlOptions =>
				{
					npgsqlOptions.EnableRetryOnFailure(3);
				});
			});
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to add db context: {ex.Message}");
		}
	}

	private static void InitializeHelpers(IConfiguration configuration)
	{
		try
		{
			var initializationTasks = new Dictionary<string, Action>
			{
				{ "JWT", () => JWT.Initialize(configuration) },
				{ "PasswordHasher", () => PasswordHasher.Initialize(configuration) },
				{ "Snowflake", () => Snowflake.Initialize(configuration) },
				{ "EmailSender", () => EmailSender.Initialize(configuration) }
			};

			foreach (var task in initializationTasks)
			{
				try
				{
					task.Value();
				}
				catch (Exception ex)
				{
					throw new Exception($"Failed to initialize {task.Key} helper: {ex.Message}");
				}
			}
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to initialize helpers: {ex.Message}");
		}
	}
}