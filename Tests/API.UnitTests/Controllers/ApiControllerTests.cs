using FluentAssertions;
using API.Web.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class ApiControllerTests : TestBase
{
    private readonly ApiController _controller;
    private readonly ILogger<ApiController> _mockLogger;

    public ApiControllerTests()
    {
        // Create mock logger
        _mockLogger = GetMock<ILogger<ApiController>>();

        // Create controller with mock dependencies
        _controller = new ApiController(_mockLogger);
    }

    [Fact]
    public void Get_ReturnsOkResultWithCorrectData()
    {
        // Act
        var result = _controller.Get() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var value = result.Value as dynamic;
        ((string)value!.message).Should().Be("API is running");
        ((string)value.status).Should().Be("healthy");
        ((DateTime)value.timestamp).Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Get_LogsInformation()
    {
        // Act
        _ = _controller.Get();

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Executing GET request")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}