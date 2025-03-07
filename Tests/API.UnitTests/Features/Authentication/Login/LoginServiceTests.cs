using API.Features.Authentication.Login.Services;
using API.Shared.Models.Infrastructure.Security.JwtService;
using API.Shared.Enums.Entities.User;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;
using NSubstitute.ExceptionExtensions;

namespace API.UnitTests.Features.Authentication.Login;

public class LoginServiceTests : TestBase
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
        long userId = 123)
    {
        var user = CreateTestUser(id: userId);

        return new API.Infrastructure.Database.Entities.Authentication.Session
        {
            User = user,
            Target = SessionTarget.User,
            Tokens = new List<Token>()
        };
    }

    #endregion

    #region LoginAsync Tests

    [Test]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsSuccessResult()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var passwordHash = "hashed-password";
        var passwordSalt = "salt";
        var refreshToken = "refresh-token-123";
        var accessToken = "access-token-123";
        var user = CreateTestUser(passwordHash: passwordHash, passwordSalt: passwordSalt);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        mockHasher!.Compare(password, passwordHash, passwordSalt)
            .Returns(true);

        mockJwtService!.GenerateRefreshToken(TokenTarget.User, (user.Id, null, null))
            .Returns(new GenerateRefreshTokenResponse
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Token generated",
                RefreshToken = refreshToken
            });

        mockJwtService!.GenerateAccessToken(refreshToken)
            .Returns(new GenerateAccessTokenResponse
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Token generated",
                AccessToken = accessToken
            });

        mockSessionRepository!.AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>())
            .Returns(Task.CompletedTask);

        mockSessionRepository!.AddTokenAsync(Arg.Any<Token>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Status, Is.EqualTo("SUCCESS"));
            Assert.That(result.AccessToken, Is.EqualTo(accessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(refreshToken));
            Assert.That(result.HttpStatusCode, Is.EqualTo(200));
        });

        await mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        mockHasher!.Received(1).Compare(password, passwordHash, passwordSalt);
        await mockSessionRepository!.Received(1).AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>());
        await mockSessionRepository!.Received(2).AddTokenAsync(Arg.Any<Token>());
    }

    [Test]
    public async Task LoginAsync_WhenUserNotFound_ReturnsUserNotFoundError()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "Password123!";
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns((User?)null);

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Invalid credentials"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        mockHasher!.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>());
    }

    [Test]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var username = "testuser";
        var password = "WrongPassword123!";
        var passwordHash = "hashed-password";
        var passwordSalt = "salt";
        var user = CreateTestUser(passwordHash: passwordHash, passwordSalt: passwordSalt);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        mockHasher!.Compare(password, passwordHash, passwordSalt)
            .Returns(false);

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Invalid credentials"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        mockHasher!.Received(1).Compare(password, passwordHash, passwordSalt);
        await mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>());
    }

    [Test]
    public async Task LoginAsync_WhenAccountNotActive_ReturnsAccountNotActiveError()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var passwordHash = "hashed-password";
        var passwordSalt = "salt";
        var user = CreateTestUser(
            passwordHash: passwordHash,
            passwordSalt: passwordSalt,
            state: UserState.PendingVerification);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("User is not active"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        mockHasher!.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>());
    }

    [Test]
    public async Task LoginAsync_WhenAccountSuspended_ReturnsAccountNotActiveError()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var passwordHash = "hashed-password";
        var passwordSalt = "salt";
        var user = CreateTestUser(
            passwordHash: passwordHash,
            passwordSalt: passwordSalt,
            state: UserState.Suspended);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("User is not active"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        mockHasher!.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>());
    }

    [Test]
    public async Task LoginAsync_WhenRefreshTokenGenerationFails_ReturnsInternalServerError()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var passwordHash = "hashed-password";
        var passwordSalt = "salt";
        var user = CreateTestUser(passwordHash: passwordHash, passwordSalt: passwordSalt);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        mockHasher!.Compare(password, passwordHash, passwordSalt)
            .Returns(true);

        mockJwtService!.GenerateRefreshToken(TokenTarget.User, (user.Id, null, null))
            .Returns(new GenerateRefreshTokenResponse
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = "Failed to generate refresh token",
                RefreshToken = null
            });

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Failed to generate refresh token"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(500));
        });

        await mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        mockHasher!.Received(1).Compare(password, passwordHash, passwordSalt);
        await mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<API.Infrastructure.Database.Entities.Authentication.Session>());
    }

    [Test]
    public async Task LoginAsync_WhenExceptionIsThrown_ReturnsInternalServerError()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var exception = new Exception("Unexpected error");
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockSessionRepository = Substitute.For<ISessionRepository>();
        var mockHasher = Substitute.For<IHasher>();
        var mockJwtService = Substitute.For<IJwtService>();
        var loginService = new LoginService(
            mockUserRepository,
            mockSessionRepository,
            mockHasher,
            mockJwtService
        );

        mockUserRepository!.GetUserByUsernameAsync(username)
            .Throws(exception);

        // Act
        var result = await loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Unexpected error"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(500));
        });
    }

    #endregion
}