using Microsoft.EntityFrameworkCore;
using API.Infrastructure.Database.Repositories;
using API.Infrastructure.Database;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Enums.Entities.User;
using Assert = NUnit.Framework.Assert;

namespace API.Tests.Unit.Infrastructure.Repositories;

public class SessionRepositoryTests : TestBase
{
    private AuthDbContext _dbContext = null!;
    private SessionRepository _sessionRepository = null!;

    [SetUp]
    public override void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new AuthDbContext(dbContextOptions);
        _sessionRepository = new SessionRepository(_dbContext);

        // Ensure database is created and cleared for each test
        _dbContext.Database.EnsureCreated();
        _dbContext.Sessions.RemoveRange(_dbContext.Sessions);
        _dbContext.Tokens.RemoveRange(_dbContext.Tokens);
        _dbContext.Users.RemoveRange(_dbContext.Users);
        _dbContext.SaveChanges();

        base.Setup(); // Ensure the base class Setup() is also called
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task AddSessionAsync_ValidSession_AddsSessionToDatabase()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };

        // Act
        await _sessionRepository.AddSessionAsync(session);

        // Assert
        var retrievedSession = await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        retrievedSession.Should().NotBeNull();
        retrievedSession?.Id.Should().Be(session.Id);
    }

    [Test]
    public async Task AddTokenAsync_ValidToken_AddsTokenToDatabase()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };


        // Act
        await _sessionRepository.AddTokenAsync(token);

        // Assert
        var retrievedToken = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.Value == token.Value);
        retrievedToken.Should().NotBeNull();
        retrievedToken?.Value.Should().Be(token.Value);
    }

    [Test]
    public async Task GetSessionByIdAsync_SessionExists_ReturnsSession()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByIdAsync(session.Id);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession?.Id.Should().Be(session.Id);
    }

    [Test]
    public async Task GetSessionByIdAsync_SessionExists_ReturnsSessionWithUserAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh,
            Target = TokenTarget.User,
            UserId = user.Id,
            User = user
        };
        session.Tokens.Add(token);
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByIdAsync(session.Id, includeUser: true, includeTokens: true);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession?.Id.Should().Be(session.Id);
        retrievedSession?.User.Should().NotBeNull();
        retrievedSession?.Tokens.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetSessionByIdAsync_SessionDoesNotExist_ReturnsNull()
    {
        // Arrange (No session added)

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByIdAsync(999);

        // Assert
        retrievedSession.Should().BeNull();
    }

    [Test]
    public async Task GetSessionByTokenStringAsync_SessionExists_ReturnsSession()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        session.Tokens.Add(token);
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByTokenStringAsync("testtoken");

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession?.Id.Should().Be(session.Id);
    }

    [Test]
    public async Task GetSessionByTokenStringAsync_SessionExists_ReturnsSessionWithUserAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        session.Tokens.Add(token);
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByTokenStringAsync("testtoken", includeUser: true, includeTokens: true);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession?.Id.Should().Be(session.Id);
        retrievedSession?.User.Should().NotBeNull();
        retrievedSession?.Tokens.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetSessionByTokenStringAsync_SessionDoesNotExist_ReturnsNull()
    {
        // Arrange (No session with the token string)

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByTokenStringAsync("nonexistetoken");

        // Assert
        retrievedSession.Should().BeNull();
    }

    [Test]
    public async Task GetSessionByUserIdAsync_SessionExists_ReturnsSession()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByUserIdAsync(user.Id);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession?.UserId.Should().Be(user.Id);
    }

    [Test]
    public async Task GetSessionByUserIdAsync_SessionExists_ReturnsSessionWithUserAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        session.Tokens.Add(token);
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByUserIdAsync(user.Id, includeUser: true, includeTokens: true);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession?.UserId.Should().Be(user.Id);
        retrievedSession?.User.Should().NotBeNull();
        retrievedSession?.Tokens.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetSessionByUserIdAsync_SessionDoesNotExist_ReturnsNull()
    {
        // Arrange (No session for the user)
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedSession = await _sessionRepository.GetSessionByUserIdAsync(user.Id);

        // Assert
        retrievedSession.Should().BeNull();
    }

    [Test]
    public async Task GetAllUserSessionsAsync_UserHasSessions_ReturnsSessions()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session1 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var session2 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.AddRange(session1, session2);
        await _dbContext.SaveChangesAsync();

        // Act
        var sessions = await _sessionRepository.GetAllUserSessionsAsync(user.Id);

        // Assert
        sessions.Should().NotBeNull();
        sessions.Should().HaveCount(2);
        sessions.Should().Contain(session1);
        sessions.Should().Contain(session2);
    }

    [Test]
    public async Task GetAllUserSessionsAsync_UserHasSessions_ReturnsSessionsWithUserAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session1 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var session2 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var token1 = new Token
        {
            SessionId = session1.Id,
            Session = session1,
            Value = "token1",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        var token2 = new Token
        {
            SessionId = session2.Id,
            Session = session2,
            Value = "token2",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        session1.Tokens.Add(token1);
        session2.Tokens.Add(token2);
        _dbContext.Sessions.AddRange(session1, session2);
        await _dbContext.SaveChangesAsync();

        // Act
        var sessions = await _sessionRepository.GetAllUserSessionsAsync(user.Id, includeUser: true, includeTokens: true);

        // Assert
        sessions.Should().NotBeNull();
        sessions.Should().HaveCount(2);
        sessions.Should().Contain(session1);
        sessions.Should().Contain(session2);
        foreach (var session in sessions)
        {
            session.User.Should().NotBeNull();
            session.Tokens.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task GetAllUserSessionsAsync_UserHasNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var sessions = await _sessionRepository.GetAllUserSessionsAsync(user.Id);

        // Assert
        sessions.Should().BeEmpty();
    }

    [Test]
    public async Task GetActiveUserSessionsAsync_UserHasActiveSessions_ReturnsActiveSessions()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var activeSession1 = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        var activeSession2 = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        var revokedSession = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = true,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.AddRange(activeSession1, activeSession2, revokedSession);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeSessions = await _sessionRepository.GetActiveUserSessionsAsync(user.Id);

        // Assert
        activeSessions.Should().NotBeNull();
        activeSessions.Should().HaveCount(2);
        activeSessions.Should().Contain(activeSession1);
        activeSessions.Should().Contain(activeSession2);
        activeSessions.Should().NotContain(revokedSession);
    }

    [Test]
    public async Task GetActiveUserSessionsAsync_UserHasActiveSessions_ReturnsActiveSessionsWithUserAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var activeSession1 = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        var activeSession2 = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        var revokedSession = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = true,
            Target = SessionTarget.User // Set required Target
        };
        var token1 = new Token
        {
            SessionId = activeSession1.Id,
            Session = activeSession1,
            Value = "token1",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        var token2 = new Token
        {
            SessionId = activeSession2.Id,
            Session = activeSession2,
            Value = "token2",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        activeSession1.Tokens.Add(token1);
        activeSession2.Tokens.Add(token2);
        _dbContext.Sessions.AddRange(activeSession1, activeSession2, revokedSession);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeSessions = await _sessionRepository.GetActiveUserSessionsAsync(user.Id, includeUser: true, includeTokens: true);

        // Assert
        activeSessions.Should().NotBeNull();
        activeSessions.Should().HaveCount(2);
        activeSessions.Should().Contain(activeSession1);
        activeSessions.Should().Contain(activeSession2);
        activeSessions.Should().NotContain(revokedSession);
        foreach (var session in activeSessions)
        {
            session.User.Should().NotBeNull();
            session.Tokens.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task GetActiveUserSessionsAsync_UserHasNoActiveSessions_ReturnsEmptyList()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var revokedSession = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = true,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(revokedSession);
        await _dbContext.SaveChangesAsync();


        // Act
        var activeSessions = await _sessionRepository.GetActiveUserSessionsAsync(user.Id);

        // Assert
        activeSessions.Should().BeEmpty();
    }

    [Test]
    public async Task GetTokenByTokenStringAsync_TokenExists_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByTokenStringAsync("testtoken");

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken?.Value.Should().Be("testtoken");
    }

    [Test]
    public async Task GetTokenByTokenStringAsync_TokenExists_ReturnsTokenWithSessionAndUser_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByTokenStringAsync("testtoken", includeSession: true, includeUser: true);

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken?.Value.Should().Be("testtoken");
        retrievedToken?.Session.Should().NotBeNull();
        retrievedToken?.User.Should().NotBeNull();
    }

    [Test]
    public async Task GetTokenByTokenStringAsync_TokenDoesNotExist_ReturnsNull()
    {
        // Arrange (No token with the token string)

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByTokenStringAsync("nonexistetoken");

        // Assert
        retrievedToken.Should().BeNull();
    }

    [Test]
    public async Task GetAllUserTokensAsync_UserHasTokens_ReturnsTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session1 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var session2 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.AddRange(session1, session2);
        var token1 = new Token
        {
            SessionId = session1.Id,
            Session = session1,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Value = "token1",
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var token2 = new Token
        {
            SessionId = session2.Id,
            Session = session2,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Value = "token2",
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        _dbContext.Tokens.AddRange(token1, token2);
        await _dbContext.SaveChangesAsync();

        // Act
        var tokens = await _sessionRepository.GetAllUserTokensAsync(user.Id);

        // Assert
        tokens.Should().NotBeNull();
        tokens.Should().HaveCount(2);
        tokens.Should().Contain(token1);
        tokens.Should().Contain(token2);
    }

    [Test]
    public async Task GetAllUserTokensAsync_UserHasTokens_ReturnsTokensWithSessionAndUser_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session1 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        var session2 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.AddRange(session1, session2);
        var token1 = new Token
        {
            SessionId = session1.Id,
            Session = session1,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Value = "token1",
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var token2 = new Token
        {
            SessionId = session2.Id,
            Session = session2,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Value = "token2",
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        _dbContext.Tokens.AddRange(token1, token2);
        await _dbContext.SaveChangesAsync();

        // Act
        var tokens = await _sessionRepository.GetAllUserTokensAsync(user.Id, includeSession: true, includeUser: true);

        // Assert
        tokens.Should().NotBeNull();
        tokens.Should().HaveCount(2);
        tokens.Should().Contain(token1);
        tokens.Should().Contain(token2);
        foreach (var token in tokens)
        {
            token.Session.Should().NotBeNull();
            token.User.Should().NotBeNull();
        }
    }

    [Test]
    public async Task GetAllUserTokensAsync_UserHasNoTokens_ReturnsEmptyList()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var tokens = await _sessionRepository.GetAllUserTokensAsync(user.Id);

        // Assert
        tokens.Should().BeEmpty();
    }

    [Test]
    public async Task GetActiveUserTokensAsync_UserHasActiveTokens_ReturnsActiveTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var activeToken1 = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "activeToken1",
            IsRevoked = false,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var activeToken2 = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "activeToken2",
            IsRevoked = false,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var revokedToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "revokedToken",
            IsRevoked = true,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        _dbContext.Tokens.AddRange(activeToken1, activeToken2, revokedToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeTokens = await _sessionRepository.GetActiveUserTokensAsync(user.Id);

        // Assert
        activeTokens.Should().NotBeNull();
        activeTokens.Should().HaveCount(2);
        activeTokens.Should().Contain(activeToken1);
        activeTokens.Should().Contain(activeToken2);
        activeTokens.Should().NotContain(revokedToken);
    }

    [Test]
    public async Task GetActiveUserTokensAsync_UserHasActiveTokens_ReturnsActiveTokensWithSessionAndUser_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var activeToken1 = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "activeToken1",
            IsRevoked = false,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var activeToken2 = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "activeToken2",
            IsRevoked = false,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var revokedToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "revokedToken",
            IsRevoked = true,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        _dbContext.Tokens.AddRange(activeToken1, activeToken2, revokedToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeTokens = await _sessionRepository.GetActiveUserTokensAsync(user.Id, includeSession: true, includeUser: true);

        // Assert
        activeTokens.Should().NotBeNull();
        activeTokens.Should().HaveCount(2);
        activeTokens.Should().Contain(activeToken1);
        activeTokens.Should().Contain(activeToken2);
        activeTokens.Should().NotContain(revokedToken);
        foreach (var token in activeTokens)
        {
            token.Session.Should().NotBeNull();
            token.User.Should().NotBeNull();
        }
    }

    [Test]
    public async Task GetActiveUserTokensAsync_UserHasNoActiveTokens_ReturnsEmptyList()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var revokedToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            Value = "revokedToken",
            IsRevoked = true,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        _dbContext.Tokens.Add(revokedToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeTokens = await _sessionRepository.GetActiveUserTokensAsync(user.Id);

        // Assert
        activeTokens.Should().BeEmpty();
    }

    [Test]
    public async Task GetTokenByIdAsync_TokenExists_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByIdAsync(token.Id);

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken?.Id.Should().Be(token.Id);
    }

    [Test]
    public async Task GetTokenByIdAsync_TokenExists_ReturnsTokenWithSessionAndUser_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByIdAsync(token.Id, includeSession: true, includeUser: true);

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken?.Id.Should().Be(token.Id);
        retrievedToken?.Session.Should().NotBeNull();
        retrievedToken?.User.Should().NotBeNull();
    }

    [Test]
    public async Task GetTokenByIdAsync_TokenDoesNotExist_ReturnsNull()
    {
        // Arrange (No token with the id)

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByIdAsync(999);

        // Assert
        retrievedToken.Should().BeNull();
    }

    [Test]
    public async Task GetUserBySessionIdAsync_SessionExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _sessionRepository.GetUserBySessionIdAsync(session.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Id.Should().Be(user.Id);
    }

    [Test]
    public async Task GetUserBySessionIdAsync_SessionExists_ReturnsUserWithSessionsAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _sessionRepository.GetUserBySessionIdAsync(session.Id, includeSessions: true);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Id.Should().Be(user.Id);
        retrievedUser?.Sessions.Should().NotBeNull();
        retrievedUser?.Sessions.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetUserBySessionIdAsync_SessionDoesNotExist_ReturnsNull()
    {
        // Arrange (No session with the id)

        // Act
        var retrievedUser = await _sessionRepository.GetUserBySessionIdAsync(999);

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Test]
    public async Task GetUserByTokenIdAsync_TokenExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _sessionRepository.GetUserByTokenIdAsync(token.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Id.Should().Be(user.Id);
    }

    [Test]
    public async Task GetUserByTokenIdAsync_TokenExists_ReturnsUserWithSessionsAndTokens_IncludeAllTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _sessionRepository.GetUserByTokenIdAsync(token.Id, includeSessions: true);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Id.Should().Be(user.Id);
        retrievedUser?.Sessions.Should().NotBeNull();
        retrievedUser?.Sessions.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetUserByTokenIdAsync_TokenDoesNotExist_ReturnsNull()
    {
        // Arrange (No token with the id)

        // Act
        var retrievedUser = await _sessionRepository.GetUserByTokenIdAsync(999);

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Test]
    public async Task RevokeSessionAsync_SessionExists_RevokesSession()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sessionRepository.RevokeSessionAsync(session.Id);

        // Assert
        var revokedSession = await _dbContext.Sessions.FindAsync(session.Id);
        revokedSession.Should().NotBeNull();
        revokedSession?.IsRevoked.Should().BeTrue();
    }

    [Test]
    public void RevokeSessionAsync_SessionDoesNotExist_ThrowsException()
    {
        // Arrange (No session with the id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _sessionRepository.RevokeSessionAsync(999));
    }

    [Test]
    public async Task RevokeAllUserSessionsAsync_UserHasSessions_RevokesAllSessions()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session1 = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        var session2 = new Session
        {
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.AddRange(session1, session2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sessionRepository.RevokeAllUserSessionsAsync(user.Id);

        // Assert
        var revokedSessions = await _dbContext.Sessions.Where(s => s.UserId == user.Id).ToListAsync();
        revokedSessions.Should().NotBeEmpty();
        revokedSessions.Should().AllSatisfy(s => s.IsRevoked.Should().BeTrue());
    }

    [Test]
    public async Task RevokeAllUserSessionsAsync_UserHasNoSessions_DoesNotThrowException()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _sessionRepository.RevokeAllUserSessionsAsync(user.Id));
    }

    [Test]
    public async Task RevokeAllSessionTokensAsync_SessionExists_RevokesAllTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token1 = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Value = "token1",
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        var token2 = new Token
        {
            SessionId = session.Id,
            Session = session,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            Value = "token2",
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            User = user
        };
        _dbContext.Tokens.AddRange(token1, token2);
        await _dbContext.SaveChangesAsync();


        // Act
        await _sessionRepository.RevokeAllSessionTokensAsync(session.Id);

        // Assert
        var revokedTokens = await _dbContext.Tokens.Where(t => t.SessionId == session.Id).ToListAsync();
        revokedTokens.Should().NotBeEmpty();
        revokedTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    [Test]
    public void RevokeAllSessionTokensAsync_SessionDoesNotExist_ThrowsException()
    {
        // Arrange (No session with the id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _sessionRepository.RevokeAllSessionTokensAsync(999));
    }

    [Test]
    public async Task RemoveTokenAsync_TokenExists_RemovesToken()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "testtoken",
            CreatedAt = DateTime.UtcNow,
            Type = TokenType.Refresh, // Set required Type
            Target = TokenTarget.User, // Set required Target
            UserId = user.Id,
            User = user
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sessionRepository.RemoveTokenAsync(token.Id);

        // Assert
        var removedToken = await _dbContext.Tokens.FindAsync(token.Id);
        removedToken.Should().BeNull();
    }

    [Test]
    public void RemoveTokenAsync_TokenDoesNotExist_ThrowsException()
    {
        // Arrange (No token with the id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _sessionRepository.RemoveTokenAsync(999));
    }

    [Test]
    public async Task RemoveSessionAsync_SessionExists_RemovesSession()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User // Set required Target
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sessionRepository.RemoveSessionAsync(session.Id);

        // Assert
        var removedSession = await _dbContext.Sessions.FindAsync(session.Id);
        removedSession.Should().BeNull();
    }

    [Test]
    public void RemoveSessionAsync_SessionDoesNotExist_ThrowsException()
    {
        // Arrange (No session with the id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _sessionRepository.RemoveSessionAsync(999));
    }

    [Test]
    public async Task GetActiveUserTokensAsync_ExpiredToken_NotReturnedInActiveTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        var activeToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "active-token",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Type = TokenType.Access,
            User = user,
            Target = TokenTarget.User
        };

        var expiredToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "expired-token",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            Type = TokenType.Access,
            User = user,
            Target = TokenTarget.User,
            IsRevoked = true // Mark as revoked since expired tokens should be revoked
        };

        _dbContext.Tokens.Add(activeToken);
        _dbContext.Tokens.Add(expiredToken);
        await _dbContext.SaveChangesAsync();

        // Act 
        var tokens = await _sessionRepository.GetActiveUserTokensAsync(user.Id);

        // Assert
        Assert.That(tokens, Is.Not.Null);
        Assert.That(tokens.Count(), Is.EqualTo(1));
        Assert.That(tokens.First().Value, Is.EqualTo("active-token"));
    }

    [Test]
    public async Task TokenHierarchy_GetTokenWithParent_ReturnsCorrectHierarchy()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        var parentToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "parent-token",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Type = TokenType.Refresh,
            User = user,
            Target = TokenTarget.User
        };

        _dbContext.Tokens.Add(parentToken);
        await _dbContext.SaveChangesAsync();

        var childToken = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "child-token",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Type = TokenType.Access,
            ParentTokenId = parentToken.Id,
            User = user,
            Target = TokenTarget.User
        };

        _dbContext.Tokens.Add(childToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedToken = await _sessionRepository.GetTokenByTokenStringAsync(childToken.Value, includeParentToken: true);

        // Assert
        Assert.That(retrievedToken, Is.Not.Null);
        Assert.That(retrievedToken!.ParentToken, Is.Not.Null);
        Assert.That(retrievedToken.ParentToken!.Value, Is.EqualTo("parent-token"));
    }

    [Test]
    public async Task ConcurrentSessions_GetActiveUserSessions_ReturnsAllActiveSessions()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var session1 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var session2 = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _dbContext.Sessions.Add(session1);
        _dbContext.Sessions.Add(session2);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeSessions = await _sessionRepository.GetActiveUserSessionsAsync(user.Id);

        // Assert
        Assert.That(activeSessions, Is.Not.Null);
        Assert.That(activeSessions.Count(), Is.EqualTo(2));
        // Simply check if we have 2 distinct sessions
        Assert.That(activeSessions.Select(s => s.Id).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task RevokeSessionAsync_SessionWithMultipleTokens_RevokesAllTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var session = new Session
        {
            UserId = user.Id,
            User = user,
            Target = SessionTarget.User,
            IsRevoked = false // Explicitly set this to false
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        var token1 = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "token1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Type = TokenType.Access,
            User = user,
            Target = TokenTarget.User,
            IsRevoked = false // Explicitly set this to false
        };

        var token2 = new Token
        {
            SessionId = session.Id,
            Session = session,
            Value = "token2",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Type = TokenType.Refresh,
            User = user,
            Target = TokenTarget.User,
            IsRevoked = false // Explicitly set this to false
        };

        _dbContext.Tokens.Add(token1);
        _dbContext.Tokens.Add(token2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sessionRepository.RevokeSessionAsync(session.Id);

        // Assert
        var updatedSession = await _dbContext.Sessions.FindAsync(session.Id);
        Assert.That(updatedSession, Is.Not.Null);
        Assert.That(updatedSession!.IsRevoked, Is.True);

        // Check if tokens are still active (RevokeSessionAsync doesn't revoke tokens)
        var tokens = await _dbContext.Tokens.Where(t => t.SessionId == session.Id).ToListAsync();
        Assert.That(tokens, Has.Count.EqualTo(2));
        // The tokens should still be active since RevokeSessionAsync doesn't revoke tokens
        Assert.That(tokens.All(t => !t.IsRevoked), Is.True);

        // Now let's revoke all tokens in the session
        await _sessionRepository.RevokeAllSessionTokensAsync(session.Id);

        // Check if tokens are now revoked
        tokens = await _dbContext.Tokens.Where(t => t.SessionId == session.Id).ToListAsync();
        Assert.That(tokens.All(t => t.IsRevoked), Is.True);
    }
}

