using API.Features.Session.Refresh.Services;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Security;
using API.Shared.Models.Infrastructure.Security.JwtService;
using NSubstitute.ExceptionExtensions;

namespace API.UnitTests.Features.Session.Refresh;

public class RefreshSessionServiceTests : TestBase
{
    #region Helper Methods

    private User CreateTestUser(
        long id = 123,
        string username = "testuser",
        string firstName = "Test",
        string lastName = "User",
        string passwordHash = "hashed-password",
        string passwordSalt = "salt",
        DateTime? birthDate = null,
        UserGender gender = UserGender.Male,
        UserState state = UserState.Active)
    {
        return new User
        {
            Id = id,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            BirthDate = birthDate ?? DateTime.Now.AddYears(-20),
            Gender = gender,
            State = state
        };
    }

    private API.Infrastructure.Database.Entities.Authentication.Session CreateTestSession(
        long id = 1,
        User? user = null,
        SessionTarget target = SessionTarget.User,
        bool isRevoked = false)
    {
        return new API.Infrastructure.Database.Entities.Authentication.Session
        {
            Id = id,
            User = user ?? CreateTestUser(),
            Target = target,
            IsRevoked = isRevoked,
            Tokens = new List<Token>()
        };
    }

    private Token CreateTestToken(
        string value = "refresh-token-123",
        TokenType type = TokenType.Refresh,
        TokenTarget target = TokenTarget.User,
        API.Infrastructure.Database.Entities.Authentication.Session? session = null,
        bool isRevoked = false,
        User? user = null)
    {
        return new Token
        {
            Value = value,
            Type = type,
            Target = target,
            IsRevoked = isRevoked,
            Session = session!,
            User = user!
        };
    }

    #endregion

    #region RefreshSessionAsync Tests

    [Test]
    public async Task RefreshSessionAsync_WhenSessionNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "non-existent-token";

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(null));

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Session not found"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(404));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenSessionRevoked_ReturnsBadRequestError()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "revoked-session-token";
        var session = CreateTestSession(isRevoked: true);

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Session has been already revoked"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenTokenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "token-not-in-db";
        var session = CreateTestSession();

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        MockSessionRepository!.GetTokenByTokenStringAsync(refreshToken)
            .Returns(Task.FromResult<Token?>(null));

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Token not found"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(404));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenTokenIsNotRefreshToken_ReturnsBadRequestError()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var accessToken = "access-token-123";
        var session = CreateTestSession();
        var token = CreateTestToken(value: accessToken, type: TokenType.Access);

        MockSessionRepository!.GetSessionByTokenStringAsync(accessToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        MockSessionRepository!.GetTokenByTokenStringAsync(accessToken)
            .Returns(Task.FromResult<Token?>(token));

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(accessToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Token is not a refresh token"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenTokenRevoked_ReturnsBadRequestError()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "revoked-refresh-token";
        var session = CreateTestSession();
        var token = CreateTestToken(value: refreshToken, isRevoked: true);

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        MockSessionRepository!.GetTokenByTokenStringAsync(refreshToken)
            .Returns(Task.FromResult<Token?>(token));

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Token has already been revoked"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenRefreshTokensFails_ReturnsErrorResult()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "valid-refresh-token";
        var user = CreateTestUser();
        var session = CreateTestSession(user: user);
        var token = CreateTestToken(value: refreshToken, user: user, session: session);

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        MockSessionRepository!.GetTokenByTokenStringAsync(refreshToken)
            .Returns(Task.FromResult<Token?>(token));

        mockJwtService!.RefreshTokens(refreshToken)
            .Returns(new RefreshTokensResponse
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = "Failed to refresh tokens",
                RefreshToken = null,
                AccessToken = null
            });

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Failed to refresh tokens"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(500));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenTokensAreNull_ReturnsErrorResult()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "valid-refresh-token";
        var user = CreateTestUser();
        var session = CreateTestSession(user: user);
        var token = CreateTestToken(value: refreshToken, user: user, session: session);

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        MockSessionRepository!.GetTokenByTokenStringAsync(refreshToken)
            .Returns(Task.FromResult<Token?>(token));

        mockJwtService!.RefreshTokens(refreshToken)
            .Returns(new RefreshTokensResponse
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Tokens refreshed",
                RefreshToken = null,
                AccessToken = null
            });

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Internal server error"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(500));
        });
    }

    [Test]
    public async Task RefreshSessionAsync_WhenValidRefreshToken_ReturnsSuccessResult()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "valid-refresh-token";
        var newRefreshToken = "new-valid-refresh-token";
        var newAccessToken = "new-access-token";
        var user = CreateTestUser();
        var session = CreateTestSession(user: user);
        var token = CreateTestToken(value: refreshToken, user: user, session: session);

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(session));

        MockSessionRepository!.GetTokenByTokenStringAsync(refreshToken)
            .Returns(Task.FromResult<Token?>(token));

        mockJwtService!.RefreshTokens(refreshToken)
            .Returns(new RefreshTokensResponse
            {
                IsSuccess = true,
                Status = "Tokens refreshed successfully",
                Message = "Tokens refreshed successfully",
                RefreshToken = newRefreshToken,
                AccessToken = newAccessToken
            });

        MockSessionRepository!.RevokeAllSessionTokensAsync(session.Id)
            .Returns(Task.CompletedTask);

        MockSessionRepository!.AddTokenAsync(Arg.Any<Token>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Status, Is.EqualTo("SUCCESS"));
            Assert.That(result.Message, Is.EqualTo("Session refreshed"));
            Assert.That(result.RefreshToken, Is.EqualTo(newRefreshToken));
            Assert.That(result.AccessToken, Is.EqualTo(newAccessToken));
        });

        await MockSessionRepository!.Received(1).RevokeAllSessionTokensAsync(session.Id);
        await MockSessionRepository!.Received(2).AddTokenAsync(Arg.Any<Token>());
    }

    [Test]
    public async Task RefreshSessionAsync_WhenExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        var mockJwtService = Substitute.For<IJwtService>();
        var refreshSessionService = new RefreshSessionService(MockSessionRepository, mockJwtService);
        var refreshToken = "valid-refresh-token";
        var exceptionMessage = "Test database exception";

        MockSessionRepository!.GetSessionByTokenStringAsync(refreshToken, true)
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await refreshSessionService!.RefreshSessionAsync(refreshToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo(exceptionMessage));
            Assert.That(result.HttpStatusCode, Is.EqualTo(500));
        });
    }

    #endregion
}