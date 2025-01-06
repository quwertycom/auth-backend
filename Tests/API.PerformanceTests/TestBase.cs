using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace API.PerformanceTests;

public abstract class TestBase
{
    protected readonly string BaseUrl;
    protected readonly IServiceCollection Services;
    protected readonly ServiceProvider ServiceProvider;

    protected TestBase(string baseUrl = "http://localhost:5000")
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

    protected IClientFactory CreateHttpClientFactory()
    {
        return ClientFactory.Create(
            httpClientFactory: () => new HttpClient(),
            name: "performance_test_client"
        );
    }

    protected IScenario CreateScenario(string name, string endpoint, int ratePerSecond = 10, TimeSpan? duration = null)
    {
        var clientFactory = CreateHttpClientFactory();
        
        return ScenarioBuilder
            .CreateScenario(name, async context =>
            {
                var request = Http.CreateRequest("GET", $"{BaseUrl}{endpoint}")
                    .WithHeader("Accept", "application/json");

                var response = await Http.Send(clientFactory, request);
                
                return response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail();
            })
            .WithLoadSimulations(
                Simulation.RatePerSecond(rate: ratePerSecond, duration: duration ?? TimeSpan.FromSeconds(30))
            );
    }

    protected void RunScenario(IScenario scenario)
    {
        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
} 