using API.Common.Helpers;
using API.Web.Configuration;
using Microsoft.Extensions.Options;

namespace API.Infrastructure.Services.HostedServices;

public class SnowflakeInitializationService : IHostedService
{
    private readonly IOptions<SnowflakeSettings> _options;

    public SnowflakeInitializationService(IOptions<SnowflakeSettings> options)
    {
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Snowflake.Initialize(_options);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to clean up
        return Task.CompletedTask;
    }
} 