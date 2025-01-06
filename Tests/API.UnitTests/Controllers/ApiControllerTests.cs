using FluentAssertions;
using API.Controllers;
using API.UnitTests.Utilities;
using System.Net;

namespace API.UnitTests.Controllers;

public class ApiControllerTests : TestBase
{
    [Theory]
    [InlineData("/api")]
    public async Task Endpoints_ReturnSuccessAndCorrectContentType(string endpoint)
    {
        // Arrange
        // (using base class setup)

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task Get_ReturnsApiStatus()
    {
        // Arrange
        const string EXPECTED_MESSAGE = "API is running";

        // Act
        var response = await _client.GetAndDeserialize<ApiResponse>("/api");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should().Be(EXPECTED_MESSAGE);
        response.Status.Should().Be("healthy");
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}

public class ApiResponse
{
    public required string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Status { get; set; }
} 