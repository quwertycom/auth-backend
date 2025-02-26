using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using API.Web.Configuration;
using Npgsql;

namespace API.Web.HealthChecks;

/// <summary>
/// Health check for SMTP email service
/// </summary>
public class SmtpHealthCheck : IHealthCheck
{
    private readonly EmailSettings _settings;

    public SmtpHealthCheck(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // First check if the host can be resolved
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(_settings.Host, cancellationToken);
            }
            catch (SocketException)
            {
                return HealthCheckResult.Unhealthy($"Cannot resolve SMTP host: {_settings.Host}");
            }

            // Try to connect to the SMTP server
            using var client = new TcpClient();
            var connectTask = Task.Run(async () => await client.ConnectAsync(_settings.Host, _settings.Port, cancellationToken));
            
            // Add a timeout to avoid hanging the health check
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            
            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                return HealthCheckResult.Degraded($"Connection to {_settings.Host}:{_settings.Port} timed out");
            }
            
            if (client.Connected)
            {
                return HealthCheckResult.Healthy($"Successfully connected to {_settings.Host}:{_settings.Port}");
            }
            
            return HealthCheckResult.Unhealthy($"Could not connect to SMTP server {_settings.Host}:{_settings.Port}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SMTP health check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check for PostgreSQL database
/// </summary>
public class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly DatabaseSettings _settings;
    private readonly string _connectionString;

    public PostgreSqlHealthCheck(IOptions<DatabaseSettings> options)
    {
        _settings = options.Value;
        _connectionString = $"Host={_settings.Host};Database={_settings.Database};Username={_settings.Username};Password={_settings.Password}";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Execute a simple query to test the connection
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy($"Successfully connected to PostgreSQL database {_settings.Database} on {_settings.Host}");
        }
        catch (NpgsqlException ex)
        {
            return HealthCheckResult.Unhealthy($"PostgreSQL health check failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Unexpected error during PostgreSQL health check: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check for Docker network connectivity
/// </summary>
public class DockerNetworkHealthCheck : IHealthCheck
{
    private readonly IHostEnvironment _environment;
    
    public DockerNetworkHealthCheck(IHostEnvironment environment)
    {
        _environment = environment;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Skip check if not in Docker environment
        if (_environment.IsDevelopment() && !_environment.IsEnvironment("DockerDevelopment"))
        {
            return HealthCheckResult.Healthy("Local development environment - network check skipped");
        }
        
        try
        {
            // Check connectivity to DB container
            using var dbClient = new TcpClient();
            var dbConnectTask = Task.Run(async () => await dbClient.ConnectAsync("db", 5432, cancellationToken));
            var dbTimeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            
            if (await Task.WhenAny(dbConnectTask, dbTimeoutTask) == dbTimeoutTask)
            {
                return HealthCheckResult.Degraded("Connection to DB container timed out");
            }
            
            // Check connectivity to MailHog container
            using var mailClient = new TcpClient();
            var mailConnectTask = Task.Run(async () => await mailClient.ConnectAsync("mailhog", 1025, cancellationToken));
            var mailTimeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            
            if (await Task.WhenAny(mailConnectTask, mailTimeoutTask) == mailTimeoutTask)
            {
                return HealthCheckResult.Degraded("Connection to MailHog container timed out");
            }
            
            return HealthCheckResult.Healthy("Docker network connectivity is working");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Docker network health check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check specifically for MailHog in Docker
/// </summary>
public class MailHogHealthCheck : IHealthCheck
{
    private readonly IHostEnvironment _environment;
    
    public MailHogHealthCheck(IHostEnvironment environment)
    {
        _environment = environment;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // The host name differs between Docker and local development
            string host = _environment.IsDevelopment() && !_environment.IsEnvironment("DockerDevelopment") 
                ? "localhost" 
                : "mailhog";
            int smtpPort = 1025;
            int httpPort = 8025;
            
            // Check SMTP port connectivity
            using var smtpClient = new TcpClient();
            var smtpConnectTask = Task.Run(async () => await smtpClient.ConnectAsync(host, smtpPort, cancellationToken));
            var smtpTimeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            
            if (await Task.WhenAny(smtpConnectTask, smtpTimeoutTask) == smtpTimeoutTask)
            {
                return HealthCheckResult.Degraded($"Connection to MailHog SMTP port on {host}:{smtpPort} timed out");
            }
            
            // Check HTTP API port connectivity
            using var httpClient = new TcpClient();
            var httpConnectTask = Task.Run(async () => await httpClient.ConnectAsync(host, httpPort, cancellationToken));
            var httpTimeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            
            if (await Task.WhenAny(httpConnectTask, httpTimeoutTask) == httpTimeoutTask)
            {
                return HealthCheckResult.Degraded($"Connection to MailHog HTTP API on {host}:{httpPort} timed out");
            }
            
            return HealthCheckResult.Healthy($"Successfully connected to MailHog on {host}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"MailHog health check failed: {ex.Message}");
        }
    }
} 