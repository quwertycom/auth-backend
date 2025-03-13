using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Features.Authentication.Login.Models.Contracts;
using API.Features.Session.Refresh.Models.Contracts;
using API.Shared.Interfaces.Database.Repositories;
using NUnit.Framework;

namespace API.Tests.Integration.Session;

[TestFixture]
public class RefreshSessionTests : TestBase
{
    [Test]
    public async Task RefreshSession_Endpoint_Should_BeAccessible()
    {
        // Act: Simply check if the endpoint exists and responds
        var response = await _client.PostAsync("/api/session/refresh", null);

        // Assert: The endpoint exists even if it returns method not allowed or bad request
        Assert.IsFalse(response.StatusCode == HttpStatusCode.NotFound, "Refresh session endpoint should exist");
    }

    [Test]
    public async Task RefreshSession_WithInvalidCredentials_ShouldReturnNotFound()
    {
        // Arrange - Use a refresh token that should be handled gracefully
        var refreshRequest = new RefreshSessionRequest
        {
            Token = "123" // Valid format but likely doesn't exist
        };

        // Act
        var response = await PostAsync("/api/session/refresh", refreshRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Expected not found for non-existent session");
    }

    [Test]
    public async Task RefreshSession_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        // Arrange - Use a non-existent refresh token
        var refreshRequest = new RefreshSessionRequest
        {
            Token = "999999999" // Non-existent refresh token
        };

        // Act
        var response = await PostAsync("/api/session/refresh", refreshRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Expected not found for non-existent session");
    }

    [Test]
    public async Task RefreshSession_WithMissingRefreshToken_ShouldReturnBadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshSessionRequest
        {
            Token = null! // Missing refresh token
        };

        // Act
        var response = await PostAsync("/api/session/refresh", refreshRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Expected bad request for missing refresh token");
    }

    [Test]
    public async Task RefreshSession_WithInvalidRefreshTokenFormat_ShouldReturnNotFound()
    {
        // Arrange
        var refreshRequest = new RefreshSessionRequest
        {
            Token = "not-a-number" // Invalid format - should be numeric
        };

        // Act
        var response = await PostAsync("/api/session/refresh", refreshRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Expected not found for invalid refresh token format");
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