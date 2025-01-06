using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace API.PerformanceTests;

public abstract class TestBase
{
    protected readonly string BaseUrl;
    protected readonly IServiceCollection Services;
    protected readonly ServiceProvider ServiceProvider;

    protected TestBase(string baseUrl = "http://localhost:8000")
    {
        BaseUrl = baseUrl;
        Services = new ServiceCollection();
        ConfigureServices(Services);
        ServiceProvider = Services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override this in derived classes to configure specific services
    }

    protected ScenarioProps CreateScenario(string name, string endpoint, int ratePerSecond = 10, TimeSpan? duration = null)
    {
        var httpClient = new HttpClient();

        return Scenario.Create(name, async context =>
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{endpoint}");
                request.Headers.Add("Accept", "application/json");

                var response = await httpClient.SendAsync(request);

                return response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail();
            }
            catch (Exception ex)
            {
                return Response.Fail(ex);
            }
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.Inject(
                rate: ratePerSecond,
                interval: TimeSpan.FromSeconds(1),
                during: duration ?? TimeSpan.FromSeconds(30)
            )
        );
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
} 