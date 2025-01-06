using NBomber.Contracts;
using NBomber.CSharp;
using FluentAssertions;
using System.Net.Http;

namespace API.PerformanceTests.Controllers;

public class ApiControllerTests : TestBase
{
    [Fact]
    public async Task Api_UnderLoad_MaintainsPerformance()
    {
        // Arrange - First check if API is running
        using var checkClient = new HttpClient();
        try
        {
            var response = await checkClient.GetAsync($"{BaseUrl}/api");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
            throw new SkipException("API is not running. Please start the API before running performance tests.");
        }

        // Create performance test scenario
        var scenario = CreateScenario(
            name: "api_health_check",
            endpoint: "/api",
            ratePerSecond: 50,  // 50 requests per second
            duration: TimeSpan.FromSeconds(30)  // Run for 30 seconds
        );

        // Act & Assert
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithTestName("API Health Check Load Test")
            .WithTestSuite("API Performance Tests")
            .Run();

        // Get the scenario stats
        var scenarioStats = stats.ScenarioStats[0];

        // Assert success rate (should be 100%)
        scenarioStats.Ok.Request.Count.Should().BeGreaterThan(0);
        scenarioStats.Fail.Request.Count.Should().Be(0);
        
        // Assert throughput (should handle our target rate)
        scenarioStats.Ok.Request.RPS.Should().BeGreaterThanOrEqualTo(45); // Allow for small variations
    }
}

public class SkipException : Exception
{
    public SkipException(string message) : base(message) { }
} 