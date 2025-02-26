using API.Common.Helpers;
using API.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace API.HostedServices;

public class HasherInitializationService : IHostedService
{
    private readonly IOptions<PasswordHasherSettings> _options;

    public HasherInitializationService(IOptions<PasswordHasherSettings> options)
    {
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Hasher.Initialize(_options);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to clean up
        return Task.CompletedTask;
    }
} 