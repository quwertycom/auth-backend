using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Features.Session.Refresh.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using NUnit.Framework;

namespace API.Tests.Functional.Features.Session;

[TestFixture]
public class RefreshWorkflowTests : TestBase
{
    [Test]
    public async Task RefreshSession_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/session/refresh");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task RefreshSession_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var (refreshToken, sessionId) = await CreateValidRefreshTokenAsync();
        var request = new RefreshSessionRequest
        {
            Token = refreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<RefreshSessionResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.Status.Should().Be("SUCCESS");
    }

    [Test]
    public async Task RefreshSession_WithRevokedToken_ShouldReturnBadRequest()
    {
        // Arrange
        var (refreshToken, _) = await CreateRevokedRefreshTokenAsync();
        var request = new RefreshSessionRequest
        {
            Token = refreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<RefreshSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("revoked");
    }

    [Test]
    public async Task RefreshSession_WithInvalidToken_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RefreshSessionRequest
        {
            Token = "invalid-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadFromJsonAsync<RefreshSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("not found");
    }

    [Test]
    public async Task RefreshSession_WithAccessToken_ShouldReturnBadRequest()
    {
        // Arrange
        var (accessToken, _) = await CreateAccessTokenAsync();
        var request = new RefreshSessionRequest
        {
            Token = accessToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/refresh", request);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<RefreshSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("not a refresh token");
    }

    [Test]
    public async Task RefreshSession_WithMissingToken_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RefreshSessionRequest
        {
            Token = string.Empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Console.WriteLine(JsonSerializer.Serialize(content));

        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Be("Validation Error");
        content.Details.Should().HaveCount(1);
        content.Details.Should().ContainKey("token");
        content.Details.Should().NotBeNull();
        content.Details?["token"].Should().NotBeNull();
        content.Details?["token"].Should().HaveCount(1);
        content.Details?["token"][0].Should().Be("Token is required!");
    }

    [Test]
    public async Task RefreshSession_WithExpiredToken_ShouldReturnBadRequest()
    {
        // Arrange
        var (refreshToken, _) = await CreateExpiredRefreshTokenAsync();
        var request = new RefreshSessionRequest
        {
            Token = refreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/session/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<RefreshSessionResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("expired");
    }

    private async Task<(string token, long sessionId)> CreateValidRefreshTokenAsync()
    {
        var sessionRepo = GetRequiredService<ISessionRepository>();
        var userRepo = GetRequiredService<IUserRepository>();
        var jwtService = GetRequiredService<IJwtService>();

        // Create user
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        // Create session
        var session = _generate.NewSession(user: user);
        await sessionRepo.AddSessionAsync(session);

        // Generate refresh token
        var refreshTokenResult = jwtService.GenerateRefreshToken(
            TokenTarget.User,
            (userId: user.Id, accountId: null, applicationId: null)
        );

        if (!refreshTokenResult.IsSuccess || refreshTokenResult.RefreshToken == null)
        {
            throw new Exception("Failed to generate refresh token");
        }

        // Create token entity
        var token = _generate.NewToken(
            value: refreshTokenResult.RefreshToken,
            type: TokenType.Refresh,
            session: session,
            user: user,
            expiresAt: DateTime.UtcNow.AddDays(30),
            isRevoked: false
        );
        await sessionRepo.AddTokenAsync(token);

        return (refreshTokenResult.RefreshToken, session.Id);
    }

    private async Task<(string token, long sessionId)> CreateRevokedRefreshTokenAsync()
    {
        var (token, sessionId) = await CreateValidRefreshTokenAsync();
        var sessionRepo = GetRequiredService<ISessionRepository>();

        // Revoke the token
        await sessionRepo.RevokeAllSessionTokensAsync(sessionId);

        return (token, sessionId);
    }

    private async Task<(string token, long sessionId)> CreateAccessTokenAsync()
    {
        // First create a valid refresh token
        var (refreshToken, sessionId) = await CreateValidRefreshTokenAsync();
        var jwtService = GetRequiredService<IJwtService>();
        var sessionRepo = GetRequiredService<ISessionRepository>();

        // Generate access token from the refresh token
        var accessTokenResult = jwtService.GenerateAccessToken(refreshToken);

        if (!accessTokenResult.IsSuccess || accessTokenResult.AccessToken == null)
        {
            throw new Exception("Failed to generate access token");
        }

        // Create token entity and link it to the session
        var session = await sessionRepo.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            throw new Exception("Session not found");
        }

        var token = _generate.NewToken(
            value: accessTokenResult.AccessToken,
            type: TokenType.Access,
            session: session,
            user: session.User,
            expiresAt: DateTime.UtcNow.AddMinutes(15)
        );
        await sessionRepo.AddTokenAsync(token);

        return (accessTokenResult.AccessToken, sessionId);
    }

    private async Task<(string token, long sessionId)> CreateExpiredRefreshTokenAsync()
    {
        var sessionRepo = GetRequiredService<ISessionRepository>();
        var userRepo = GetRequiredService<IUserRepository>();
        var jwtService = GetRequiredService<IJwtService>();

        // Create user
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        // Create session
        var session = _generate.NewSession(user: user);
        await sessionRepo.AddSessionAsync(session);

        // Generate refresh token
        var refreshTokenResult = jwtService.GenerateRefreshToken(
            TokenTarget.User,
            (userId: user.Id, accountId: null, applicationId: null)
        );

        if (!refreshTokenResult.IsSuccess || refreshTokenResult.RefreshToken == null)
        {
            throw new Exception("Failed to generate refresh token");
        }

        // Create token entity with expired date
        var token = _generate.NewToken(
            value: refreshTokenResult.RefreshToken,
            type: TokenType.Refresh,
            session: session,
            user: user,
            expiresAt: DateTime.UtcNow.AddDays(-1) // Already expired
        );
        await sessionRepo.AddTokenAsync(token);

        return (refreshTokenResult.RefreshToken, session.Id);
    }
}
