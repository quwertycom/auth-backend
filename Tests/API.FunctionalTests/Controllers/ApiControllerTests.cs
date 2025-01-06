using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace API.FunctionalTests.Controllers;

public class ApiControllerTests : TestBase
{
    [Fact]
    public async Task Api_Endpoint_ReturnsExpectedResponse()
    {
        // Act
        var response = await GetAsync("/api");
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");

        content.Should().NotBeNull();
        content!.Message.Should().Be("API is running");
        content.Status.Should().Be("healthy");
        content.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}

public class ApiResponse
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
} 