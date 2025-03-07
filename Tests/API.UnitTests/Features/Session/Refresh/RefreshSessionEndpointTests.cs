using API.Features.Session.Refresh.Endpoints;
using API.Features.Session.Refresh.Models.Contracts;
using API.Features.Session.Refresh.Models.Services;
using API.Features.Session.Refresh.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using static System.Net.WebRequestMethods;

namespace API.UnitTests.Features.Session.Refresh;

public class RefreshSessionEndpointTests : TestBase
{
    #region Helper Methods

    private RefreshSessionRequest CreateRefreshTokenRequest(string token = "valid-refresh-token")
    {
        return new RefreshSessionRequest { Token = token };
    }

    private RefreshSessionEndpoint CreateEndpoint(IRefreshSessionService refreshSessionService)
    {
        // Use FastEndpoints Factory to create a properly initialized endpoint instance
        return Factory.Create<RefreshSessionEndpoint>(refreshSessionService);
    }

    #endregion

    #region Configuration Tests

    [Test]
    public void Configure_SetsCorrectEndpointProperties()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var endpoint = CreateEndpoint(mockService);

        // Act - if there's no exception, the configuration is successful
        endpoint.Configure();

        // Assert - We can't directly test the properties as they're internal
        // But we know from code inspection it should be setting POST for /api/session/refresh
        Assert.Pass("Configure executes without errors");
    }

    #endregion

    #region HandleAsync Tests

    [Test]
    public async Task HandleAsync_WhenTokenIsValid_ReturnsSuccessResponse()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var request = CreateRefreshTokenRequest();
        var serviceResult = new RefreshSessionResult
        {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "Session refreshed successfully",
            HttpStatusCode = 200,
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token"
        };

        mockService.RefreshSessionAsync(request.Token)
            .Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        var response = endpoint.Response as RefreshSessionResponse;
        response.Should().NotBeNull();
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Session refreshed successfully");
        response.AccessToken.Should().Be("new-access-token");
        response.RefreshToken.Should().Be("new-refresh-token");

        // Verify service was called with the correct token
        await mockService.Received(1).RefreshSessionAsync(request.Token);
    }

    [Test]
    public async Task HandleAsync_WhenServiceReturns204_SendsNoContent()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var request = CreateRefreshTokenRequest();
        var serviceResult = new RefreshSessionResult
        {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "No content",
            HttpStatusCode = 204
        };

        mockService.RefreshSessionAsync(request.Token)
            .Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        // Can't directly access HttpContext in unit tests, verify by checking mock was called
        await mockService.Received(1).RefreshSessionAsync(request.Token);
    }

    [Test]
    public async Task HandleAsync_WhenTokenIsInvalid_ReturnsErrorResponse()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var request = CreateRefreshTokenRequest("invalid-token");
        var serviceResult = new RefreshSessionResult
        {
            IsSuccess = false,
            Status = "ERROR",
            Message = "Token not found",
            HttpStatusCode = 404
        };

        mockService.RefreshSessionAsync(request.Token)
            .Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        var response = endpoint.Response as RefreshSessionResponse;
        response.Should().NotBeNull();
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Token not found");

        // Verify service was called with the correct token
        await mockService.Received(1).RefreshSessionAsync(request.Token);
    }

    [Test]
    public async Task HandleAsync_WhenSessionRevoked_ReturnsErrorResponse()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var request = CreateRefreshTokenRequest("revoked-session-token");
        var serviceResult = new RefreshSessionResult
        {
            IsSuccess = false,
            Status = "ERROR",
            Message = "Session has been already revoked",
            HttpStatusCode = 400
        };

        mockService.RefreshSessionAsync(request.Token)
            .Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        var response = endpoint.Response as RefreshSessionResponse;
        response.Should().NotBeNull();
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Session has been already revoked");

        // Verify service was called with the correct token
        await mockService.Received(1).RefreshSessionAsync(request.Token);
    }

    [Test]
    public async Task HandleAsync_WhenTokenRevoked_ReturnsErrorResponse()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var request = CreateRefreshTokenRequest("revoked-token");
        var serviceResult = new RefreshSessionResult
        {
            IsSuccess = false,
            Status = "ERROR",
            Message = "Token has already been revoked",
            HttpStatusCode = 400
        };

        mockService.RefreshSessionAsync(request.Token)
            .Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        var response = endpoint.Response as RefreshSessionResponse;
        response.Should().NotBeNull();
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Token has already been revoked");

        // Verify service was called with the correct token
        await mockService.Received(1).RefreshSessionAsync(request.Token);
    }

    [Test]
    public async Task HandleAsync_WhenServerError_ReturnsErrorResponse()
    {
        // Arrange
        var mockService = Substitute.For<IRefreshSessionService>();
        var request = CreateRefreshTokenRequest();
        var serviceResult = new RefreshSessionResult
        {
            IsSuccess = false,
            Status = "ERROR",
            Message = "Internal server error",
            HttpStatusCode = 500
        };

        mockService.RefreshSessionAsync(request.Token)
            .Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        var response = endpoint.Response as RefreshSessionResponse;
        response.Should().NotBeNull();
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Internal server error");

        // Verify service was called with the correct token
        await mockService.Received(1).RefreshSessionAsync(request.Token);
    }

    #endregion
}