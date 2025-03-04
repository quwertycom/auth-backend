using Microsoft.EntityFrameworkCore;
using API.Infrastructure.Database;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;
using API.Shared.Enums.Entities.Authentication;

namespace API.UnitTests.Infrastructure;

public class AuthDbContextTests : TestBase
{
    private AuthDbContext _dbContext = null!;

    [SetUp]
    public override void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestAuthDbContext")
            .Options;

        _dbContext = new AuthDbContext(dbContextOptions);

        // Ensure database is created and cleared for each test
        _dbContext.Database.EnsureCreated();
        _dbContext.Database.EnsureDeleted(); // Start with a clean database for each test
        _dbContext.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task CanAddAndGetUser()
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

        // Act
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var retrievedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Username.Should().Be("testuser");
    }

    [Test]
    public async Task CanAddSessionAndTokenWithUserRelationship()
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
            Target = SessionTarget.User
        };
        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        var token = new Token
        {
            SessionId = session.Id,
            Session = session,
            UserId = user.Id,
            User = user,
            Value = "testtoken",
            Type = TokenType.Refresh,
            Target = TokenTarget.User,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Tokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedToken = await _dbContext.Tokens
            .Include(t => t.Session)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Value == "testtoken");

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken?.Session.Should().NotBeNull();
        retrievedToken?.User.Should().NotBeNull();
        retrievedToken?.Session.UserId.Should().Be(user.Id);
        retrievedToken?.User.Username.Should().Be("testuser");
    }

    [Test]
    public async Task CanAddEmailVerificationRequestWithUserAndEmailAddressRelationship()
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

        var emailAddress = new EmailAddress
        {
            UserId = user.Id,
            User = user,
            Value = "test@example.com",
            Type = EmailType.Primary,
            State = EmailState.PendingVerification
        };
        _dbContext.EmailAddresses.Add(emailAddress);
        await _dbContext.SaveChangesAsync();

        var verificationRequest = new EmailVerificationRequest
        {
            UserId = user.Id,
            User = user,
            EmailId = emailAddress.Id,
            EmailAddress = emailAddress,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.EmailVerificationRequests.Add(verificationRequest);
        await _dbContext.SaveChangesAsync();


        // Act
        var retrievedRequest = await _dbContext.EmailVerificationRequests
            .Include(v => v.User)
            .Include(v => v.EmailAddress)
            .FirstOrDefaultAsync(v => v.Code == "123456");

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.User.Should().NotBeNull();
        retrievedRequest?.EmailAddress.Should().NotBeNull();
        retrievedRequest?.User.Username.Should().Be("testuser");
        retrievedRequest?.EmailAddress.Value.Should().Be("test@example.com");
    }

    [Test]
    public async Task CreatedAtIsSetAutomatically()
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

        // Act
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrievedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        retrievedUser?.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1)); // Check if CreatedAt is set and close to UtcNow
    }
}
