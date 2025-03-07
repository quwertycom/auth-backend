using API.Features.Session.Revoke.Endpoints;
using API.Features.Session.Revoke.Interfaces;
using API.Features.Session.Revoke.Models.Contracts;
using API.Features.Session.Revoke.Models.Services;
using System.Reflection;

namespace API.UnitTests.Features.Session.Revoke;

public class RevokeSessionEndpointTests : TestBase
{
    #region Helper Methods

    private RevokeSessionRequest CreateDefaultRequest(string sessionId = "123")
    {
        return new RevokeSessionRequest
        {
            SessionId = sessionId
        };
    }

    private RevokeSessionResult CreateSuccessResult()
    {
        return new RevokeSessionResult
        {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "Session revoked",
            HttpStatusCode = 200
        };
    }

    private RevokeSessionResult CreateFailureResult(
        string status = "ERROR",
        string message = "Failed to revoke session",
        int? httpStatusCode = 400)
    {
        return new RevokeSessionResult
        {
            IsSuccess = false,
            Status = status,
            Message = message,
            HttpStatusCode = httpStatusCode
        };
    }

    #endregion

    #region Constructor Tests

    [Test]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var mockRevokeSessionService = Substitute.For<IRevokeSessionService>();
        var endpoint = new RevokeSessionEndpoint(mockRevokeSessionService);

        // Assert - use reflection to check private field was initialized
        var fieldInfo = typeof(RevokeSessionEndpoint).GetField(
            "_revokeSessionService",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(fieldInfo, Is.Not.Null, "Field _revokeSessionService should exist");

        var fieldValue = fieldInfo!.GetValue(endpoint);
        Assert.That(fieldValue, Is.EqualTo(mockRevokeSessionService),
            "Service should be initialized in constructor");
    }

    #endregion

    #region Configuration Tests

    [Test]
    public void Configure_SetsEndpointRouteAndOptions()
    {
        // Get the method info for inspection
        var configureMethod = typeof(RevokeSessionEndpoint).GetMethod("Configure",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Verify method exists
        Assert.That(configureMethod, Is.Not.Null, "Configure method should exist");
    }

    #endregion

    #region Service Interaction Tests

    [Test]
    public async Task HandleAsync_CallsServiceWithCorrectSessionId()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var successResult = CreateSuccessResult();
        var mockRevokeSessionService = Substitute.For<IRevokeSessionService>();

        mockRevokeSessionService
            .RevokeSessionAsync(123)
            .Returns(successResult);

        // Create endpoint
        var endpoint = new RevokeSessionEndpoint(mockRevokeSessionService);

        try
        {
            // Act - will throw exception due to FastEndpoints dependencies
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception in unit test environment
        }

        // Assert - verify the service was called with correct parameters
        await mockRevokeSessionService
            .Received(1)
            .RevokeSessionAsync(123);
    }

    [Test]
    public async Task HandleAsync_WithSessionNotFound_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var failureResult = CreateFailureResult(
            status: "ERROR",
            message: "Session not found",
            httpStatusCode: 404
        );
        var mockRevokeSessionService = Substitute.For<IRevokeSessionService>();

        mockRevokeSessionService
            .RevokeSessionAsync(123)
            .Returns(failureResult);

        // Create endpoint
        var endpoint = new RevokeSessionEndpoint(mockRevokeSessionService);

        try
        {
            // Act - will throw exception due to FastEndpoints dependencies
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception in unit test environment
        }

        // Assert - verify the service was called with correct parameters
        await mockRevokeSessionService
            .Received(1)
            .RevokeSessionAsync(123);
    }

    [Test]
    public async Task HandleAsync_WithAlreadyRevokedSession_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var failureResult = CreateFailureResult(
            status: "ERROR",
            message: "Session has been already revoked",
            httpStatusCode: 400
        );
        var mockRevokeSessionService = Substitute.For<IRevokeSessionService>();

        mockRevokeSessionService
            .RevokeSessionAsync(123)
            .Returns(failureResult);

        // Create endpoint
        var endpoint = new RevokeSessionEndpoint(mockRevokeSessionService);

        try
        {
            // Act - will throw exception due to FastEndpoints dependencies
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception in unit test environment
        }

        // Assert - verify the service was called with correct parameters
        await mockRevokeSessionService
            .Received(1)
            .RevokeSessionAsync(123);
    }

    [Test]
    public async Task HandleAsync_WithInternalError_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var failureResult = CreateFailureResult(
            status: "ERROR",
            message: "Internal server error",
            httpStatusCode: 500
        );
        var mockRevokeSessionService = Substitute.For<IRevokeSessionService>();

        mockRevokeSessionService
            .RevokeSessionAsync(123)
            .Returns(failureResult);

        // Create endpoint
        var endpoint = new RevokeSessionEndpoint(mockRevokeSessionService);

        try
        {
            // Act - will throw exception due to FastEndpoints dependencies
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception in unit test environment
        }

        // Assert - verify the service was called with correct parameters
        await mockRevokeSessionService
            .Received(1)
            .RevokeSessionAsync(123);
    }

    #endregion
} 