using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Features.Authentication.Login.Models.Contracts;
using API.Features.Session.Revoke.Models.Contracts;
using API.Shared.Interfaces.Database.Repositories;
using NUnit.Framework;

namespace API.Tests.Integration.Session;

[TestFixture]
public class RevokeSessionTests : TestBase
{
    [Test]
    public async Task RevokeSession_Endpoint_Should_BeAccessible()
    {
        // Act: Simply check if the endpoint exists and responds
        var response = await _client.PostAsync("/api/session/revoke", null);

        // Assert: The endpoint exists even if it returns method not allowed or bad request
        Assert.IsFalse(response.StatusCode == HttpStatusCode.NotFound, "Revoke session endpoint should exist");
    }

    [Test]
    public async Task RevokeSession_WithInvalidCredentials_ShouldReturnNotFound()
    {
        // Arrange - Use a session ID that should be handled gracefully
        var revokeRequest = new RevokeSessionRequest
        {
            SessionId = "123" // Valid format but likely doesn't exist
        };

        // Act
        var response = await PostAsync("/api/session/revoke", revokeRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Expected not found for non-existent session");
    }

    [Test]
    public async Task RevokeSession_WithInvalidSessionId_ShouldReturnNotFound()
    {
        // Arrange - Use a non-existent session ID
        var revokeRequest = new RevokeSessionRequest
        {
            SessionId = "999999999" // Non-existent session ID
        };

        // Act
        var response = await PostAsync("/api/session/revoke", revokeRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Expected not found for non-existent session");
    }

    [Test]
    public async Task RevokeSession_WithMissingSessionId_ShouldReturnBadRequest()
    {
        // Arrange
        var revokeRequest = new RevokeSessionRequest
        {
            SessionId = null! // Missing session ID
        };

        // Act
        var response = await PostAsync("/api/session/revoke", revokeRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Expected bad request for missing session ID");
    }

    [Test]
    public async Task RevokeSession_WithInvalidSessionIdFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var revokeRequest = new RevokeSessionRequest
        {
            SessionId = "not-a-number" // Invalid format - should be numeric
        };

        // Act
        var response = await PostAsync("/api/session/revoke", revokeRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Expected bad request for invalid session ID format");
    }

    #region Helper Methods

    private async Task<HttpResponseMessage> LoginAsync(string username, string password)
    {
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        return await PostAsync("/api/authentication/login", loginRequest);
    }

    #endregion
}