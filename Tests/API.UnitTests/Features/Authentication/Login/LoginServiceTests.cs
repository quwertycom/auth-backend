using API.Features.Authentication.Login.Models.Services;
using API.Features.Authentication.Login.Services;
using API.Shared.Enums.Entities.User;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace API.UnitTests.Features.Authentication.Login;

public class LoginServiceTests : TestBase
{
    private IUserRepository? _mockUserRepository;
    private ISessionRepository? _mockSessionRepository;
    private IHasher? _mockHasher;
    private IJwtService? _mockJwtService;
    private LoginService? _loginService;

    #region Setup

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockSessionRepository = Substitute.For<ISessionRepository>();
        _mockHasher = Substitute.For<IHasher>();
        _mockJwtService = Substitute.For<IJwtService>();

        _loginService = new LoginService(
            _mockUserRepository,
            _mockSessionRepository,
            _mockHasher,
            _mockJwtService
        );
    }

    #endregion

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

    private Session CreateTestSession(
        long userId = 123)
    {
        var user = CreateTestUser(id: userId);

        return new Session
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

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        _mockHasher!.Compare(password, passwordHash, passwordSalt)
            .Returns(true);

        _mockJwtService!.GenerateRefreshToken(TokenTarget.User, (user.Id, null, null))
            .Returns((true, "SUCCESS", "Token generated", refreshToken));

        _mockJwtService!.GenerateAccessToken(refreshToken)
            .Returns((true, "SUCCESS", "Token generated", accessToken));

        _mockSessionRepository!.AddSessionAsync(Arg.Any<Session>())
            .Returns(Task.CompletedTask);

        _mockSessionRepository!.AddTokenAsync(Arg.Any<Token>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Status, Is.EqualTo("SUCCESS"));
            Assert.That(result.AccessToken, Is.EqualTo(accessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(refreshToken));
            Assert.That(result.HttpStatusCode, Is.EqualTo(200));
        });

        await _mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        _mockHasher!.Received(1).Compare(password, passwordHash, passwordSalt);
        await _mockSessionRepository!.Received(1).AddSessionAsync(Arg.Any<Session>());
        await _mockSessionRepository!.Received(2).AddTokenAsync(Arg.Any<Token>());
    }

    [Test]
    public async Task LoginAsync_WhenUserNotFound_ReturnsUserNotFoundError()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "Password123!";

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns((User?)null);

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Invalid credentials"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await _mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        _mockHasher!.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<Session>());
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

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        _mockHasher!.Compare(password, passwordHash, passwordSalt)
            .Returns(false);

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Invalid credentials"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await _mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        _mockHasher!.Received(1).Compare(password, passwordHash, passwordSalt);
        await _mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<Session>());
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

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("User is not active"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await _mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        _mockHasher!.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<Session>());
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

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("User is not active"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(401));
        });

        await _mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        _mockHasher!.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<Session>());
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

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Returns(user);

        _mockHasher!.Compare(password, passwordHash, passwordSalt)
            .Returns(true);

        _mockJwtService!.GenerateRefreshToken(TokenTarget.User, (user.Id, null, null))
            .Returns((false, "ERROR", "Failed to generate refresh token", (string?)null));

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(result.Message, Is.EqualTo("Failed to generate refresh token"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(500));
        });

        await _mockUserRepository!.Received(1).GetUserByUsernameAsync(username);
        _mockHasher!.Received(1).Compare(password, passwordHash, passwordSalt);
        await _mockSessionRepository!.DidNotReceive().AddSessionAsync(Arg.Any<Session>());
    }

    [Test]
    public async Task LoginAsync_WhenExceptionIsThrown_ReturnsInternalServerError()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var exception = new Exception("Unexpected error");

        _mockUserRepository!.GetUserByUsernameAsync(username)
            .Throws(exception);

        // Act
        var result = await _loginService!.LoginAsync(username, password, CancellationToken.None);

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