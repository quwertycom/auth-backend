using System.Net;
using System.Net.Http.Json;
using API.Features.Session.Revoke.Models.Contracts;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Interfaces.Database.Repositories;
using NUnit.Framework;

namespace API.Tests.Functional.Features.Session;

[TestFixture]
public class RevokeWorkflowTests : TestBase
{
    [Test]
    public async Task RevokeSession_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/session/revoke");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task RevokeSession_WithValidSession_ShouldReturnSuccess()
    {
        // Arrange
        var sessionId = await CreateValidSessionAsync();
        var request = new RevokeSessionRequest
        {
            SessionId = sessionId.ToString()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/revoke", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<RevokeSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.Message.Should().Be("Session revoked");
    }

    [Test]
    public async Task RevokeSession_WithAlreadyRevokedSession_ShouldReturnBadRequest()
    {
        // Arrange
        var sessionId = await CreateRevokedSessionAsync();
        var request = new RevokeSessionRequest
        {
            SessionId = sessionId.ToString()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/revoke", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<RevokeSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("already revoked");
    }

    [Test]
    public async Task RevokeSession_WithInvalidSessionId_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RevokeSessionRequest
        {
            SessionId = "999999999" // Non-existent session ID
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/revoke", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadFromJsonAsync<RevokeSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("not found");
    }

    [Test]
    public async Task RevokeSession_WithInvalidSessionIdFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RevokeSessionRequest
        {
            SessionId = "invalid-id"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/revoke", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<RevokeSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Validation Error");
    }

    [Test]
    public async Task RevokeSession_WithMissingSessionId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RevokeSessionRequest
        {
            SessionId = string.Empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/revoke", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<RevokeSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Validation Error");
    }

    private async Task<long> CreateValidSessionAsync()
    {
        var sessionRepo = GetRequiredService<ISessionRepository>();
        var userRepo = GetRequiredService<IUserRepository>();

        // Create user
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        // Create session
        var session = _generate.NewSession(user: user);
        await sessionRepo.AddSessionAsync(session);

        return session.Id;
    }

    private async Task<long> CreateRevokedSessionAsync()
    {
        var sessionId = await CreateValidSessionAsync();
        var sessionRepo = GetRequiredService<ISessionRepository>();

        // Revoke the session
        await sessionRepo.RevokeSessionAsync(sessionId);

        return sessionId;
    }
}
