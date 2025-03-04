using NUnit.Framework;
using API.Infrastructure.Database.Repositories;
using API.Infrastructure.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using API.Infrastructure.Database.Entities.Verification;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;
using System;
using System.Linq;
using Assert = NUnit.Framework.Assert;

namespace API.UnitTests.Infrastructure.Repositories;

public class VerificationRepositoryTests : TestBase
{
    private AuthDbContext _dbContext = null!;
    private VerificationRepository _verificationRepository = null!;

    [SetUp]
    public override void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestVerificationDatabase")
            .Options;

        _dbContext = new AuthDbContext(dbContextOptions);
        _verificationRepository = new VerificationRepository(_dbContext);

        // Ensure database is created and cleared for each test
        _dbContext.Database.EnsureCreated();
        _dbContext.EmailVerificationRequests.RemoveRange(_dbContext.EmailVerificationRequests);
        _dbContext.PasswordResetRequests.RemoveRange(_dbContext.PasswordResetRequests);
        _dbContext.Users.RemoveRange(_dbContext.Users);
        _dbContext.EmailAddresses.RemoveRange(_dbContext.EmailAddresses);
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
    public async Task AddEmailVerificationRequestAsync_ValidRequest_AddsRequestToDatabase()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();
        var request = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };

        // Act
        await _verificationRepository.AddEmailVerificationRequestAsync(request);

        // Assert
        var retrievedRequest = await _dbContext.EmailVerificationRequests.FirstOrDefaultAsync(r => r.Id == request.Id);
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.Id.Should().Be(request.Id);
    }

    [Test]
    public async Task AddPasswordResetRequestAsync_ValidRequest_AddsRequestToDatabase()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();
        var request = new PasswordResetRequest { CodeHash = "hashedcode", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };

        // Act
        await _verificationRepository.AddPasswordResetRequestAsync(request);

        // Assert
        var retrievedRequest = await _dbContext.PasswordResetRequests.FirstOrDefaultAsync(r => r.Id == request.Id);
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.Id.Should().Be(request.Id);
    }

    [Test]
    public async Task GetEmailVerificationRequestByCodeAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByCodeAsync("12345678");

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.Code.Should().Be("12345678");
    }

    [Test]
    public async Task GetEmailVerificationRequestByCodeAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this code)

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByCodeAsync("nonexistentcode");

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetEmailVerificationRequestByEmailIdAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByEmailIdAsync(email.Id);

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.EmailId.Should().Be(email.Id);
    }

    [Test]
    public async Task GetEmailVerificationRequestByEmailIdAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this email id)

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByEmailIdAsync(999);

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetEmailVerificationRequestByIdAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        // Generate a long ID using Snowflake
        long requestId = API.Shared.Utilities.Snowflake.Generate();
        var request = new EmailVerificationRequest { Id = requestId, Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByIdAsync(requestId);

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.Id.Should().Be(requestId); // Assert using long ID
    }

    [Test]
    public async Task GetEmailVerificationRequestByIdAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this id)
        // Generate a long ID for a non-existent request
        long nonExistentRequestId = API.Shared.Utilities.Snowflake.Generate();

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByIdAsync(nonExistentRequestId);

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetEmailVerificationRequestByEmailStringAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByEmailStringAsync("test@example.com");

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.EmailAddress.Value.Should().Be("test@example.com");
    }

    [Test]
    public async Task GetEmailVerificationRequestByEmailStringAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this email string)

        // Act
        var retrievedRequest = await _verificationRepository.GetEmailVerificationRequestByEmailStringAsync("nonexistent@example.com");

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetPasswordResetRequestByCodeHashAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new PasswordResetRequest { CodeHash = "hashedcode", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetPasswordResetRequestByCodeHashAsync("hashedcode");

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.CodeHash.Should().Be("hashedcode");
    }

    [Test]
    public async Task GetPasswordResetRequestByCodeHashAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this code hash)

        // Act
        var retrievedRequest = await _verificationRepository.GetPasswordResetRequestByCodeHashAsync("nonexistenthash");

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetPasswordResetRequestByEmailIdAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new PasswordResetRequest { CodeHash = "hashedcode", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetPasswordResetRequestByEmailIdAsync(email.Id);

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.EmailId.Should().Be(email.Id);
    }

    [Test]
    public async Task GetPasswordResetRequestByEmailIdAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this email id)

        // Act
        var retrievedRequest = await _verificationRepository.GetPasswordResetRequestByEmailIdAsync(999);

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetPasswordResetRequestByEmailStringAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new PasswordResetRequest { CodeHash = "hashedcode", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRequest = await _verificationRepository.GetPasswordResetRequestByEmailStringAsync("test@example.com");

        // Assert
        retrievedRequest.Should().NotBeNull();
        retrievedRequest?.EmailAddress.Value.Should().Be("test@example.com");
    }

    [Test]
    public async Task GetPasswordResetRequestByEmailStringAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange (No request with this email string)

        // Act
        var retrievedRequest = await _verificationRepository.GetPasswordResetRequestByEmailStringAsync("nonexistent@example.com");

        // Assert
        retrievedRequest.Should().BeNull();
    }

    [Test]
    public async Task GetAllUserPasswordResetRequestsAsync_UserHasRequests_ReturnsRequests()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request1 = new PasswordResetRequest { CodeHash = "hashedcode1", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        var request2 = new PasswordResetRequest { CodeHash = "hashedcode2", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.AddRange(request1, request2);
        await _dbContext.SaveChangesAsync();

        // Act
        var requests = await _verificationRepository.GetAllUserPasswordResetRequestsAsync(user.Id);

        // Assert
        requests.Should().NotBeNull();
        requests.Should().HaveCount(2);
        requests.Should().Contain(request1);
        requests.Should().Contain(request2);
    }

    [Test]
    public async Task GetAllUserPasswordResetRequestsAsync_UserHasNoRequests_ReturnsEmptyList()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var requests = await _verificationRepository.GetAllUserPasswordResetRequestsAsync(user.Id);

        // Assert
        requests.Should().BeEmpty();
    }

    [Test]
    public async Task GetUserActivePasswordResetRequestsAsync_UserHasActiveRequests_ReturnsActiveRequests()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var activeRequest1 = new PasswordResetRequest { CodeHash = "hashedcode1", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, IsUsed = false, User = user, EmailAddress = email };
        var activeRequest2 = new PasswordResetRequest { CodeHash = "hashedcode2", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, IsUsed = false, User = user, EmailAddress = email };
        var usedRequest = new PasswordResetRequest { CodeHash = "hashedcode3", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, IsUsed = true, User = user, EmailAddress = email };
        var expiredRequest = new PasswordResetRequest { CodeHash = "hashedcode4", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow, IsUsed = false, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.AddRange(activeRequest1, activeRequest2, usedRequest, expiredRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeRequests = await _verificationRepository.GetUserActivePasswordResetRequestsAsync(user.Id);

        // Assert
        activeRequests.Should().NotBeNull();
        activeRequests.Should().HaveCount(2);
        activeRequests.Should().Contain(activeRequest1);
        activeRequests.Should().Contain(activeRequest2);
        activeRequests.Should().NotContain(usedRequest);
        activeRequests.Should().NotContain(expiredRequest);
    }

    [Test]
    public async Task GetUserActivePasswordResetRequestsAsync_UserHasNoActiveRequests_ReturnsEmptyList()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var usedRequest = new PasswordResetRequest { CodeHash = "hashedcode3", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, IsUsed = true, User = user, EmailAddress = email };
        var expiredRequest = new PasswordResetRequest { CodeHash = "hashedcode4", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow, IsUsed = false, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.AddRange(usedRequest, expiredRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var activeRequests = await _verificationRepository.GetUserActivePasswordResetRequestsAsync(user.Id);

        // Assert
        activeRequests.Should().BeEmpty();
    }

    [Test]
    public async Task MarkEmailVerificationRequestAsUsedAsync_RequestExists_MarksAsUsed()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, IsUsed = false, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        await _verificationRepository.MarkEmailVerificationRequestAsUsedAsync(request.Id);

        // Assert
        var updatedRequest = await _dbContext.EmailVerificationRequests.FindAsync(request.Id);
        updatedRequest.Should().NotBeNull();
        updatedRequest?.IsUsed.Should().BeTrue();
    }

    [Test]
    public void MarkEmailVerificationRequestAsUsedAsync_RequestDoesNotExist_ThrowsException()
    {
        // Arrange (No request with this id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _verificationRepository.MarkEmailVerificationRequestAsUsedAsync(999));
    }

    [Test]
    public async Task MarkPasswordResetRequestAsUsedAsync_RequestExists_MarksAsUsed()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new PasswordResetRequest { CodeHash = "hashedcode", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, IsUsed = false, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        await _verificationRepository.MarkPasswordResetRequestAsUsedAsync(request.Id);

        // Assert
        var updatedRequest = await _dbContext.PasswordResetRequests.FindAsync(request.Id);
        updatedRequest.Should().NotBeNull();
        updatedRequest?.IsUsed.Should().BeTrue();
        updatedRequest?.UsedAt.Should().NotBeNull();
    }

    [Test]
    public void MarkPasswordResetRequestAsUsedAsync_RequestDoesNotExist_ThrowsException()
    {
        // Arrange (No request with this id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _verificationRepository.MarkPasswordResetRequestAsUsedAsync(999));
    }

    [Test]
    public async Task RemoveEmailVerificationRequestAsync_RequestExists_RemovesRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        await _verificationRepository.RemoveEmailVerificationRequestAsync(request.Id);

        // Assert
        var removedRequest = await _dbContext.EmailVerificationRequests.FindAsync(request.Id);
        removedRequest.Should().BeNull();
    }

    [Test]
    public void RemoveEmailVerificationRequestAsync_RequestDoesNotExist_ThrowsException()
    {
        // Arrange (No request with this id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _verificationRepository.RemoveEmailVerificationRequestAsync(999));
    }

    [Test]
    public async Task RemovePasswordResetRequestAsync_RequestExists_RemovesRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request = new PasswordResetRequest { CodeHash = "hashedcode", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Act
        await _verificationRepository.RemovePasswordResetRequestAsync(request.Id);

        // Assert
        var removedRequest = await _dbContext.PasswordResetRequests.FindAsync(request.Id);
        removedRequest.Should().BeNull();
    }

    [Test]
    public void RemovePasswordResetRequestAsync_RequestDoesNotExist_ThrowsException()
    {
        // Arrange (No request with this id)

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _verificationRepository.RemovePasswordResetRequestAsync(999));
    }

    [Test]
    public async Task RemoveAllUserEmailVerificationRequestsAsync_UserHasRequests_RemovesAllRequests()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.PendingVerification, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request1 = new EmailVerificationRequest { Code = "12345678", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        var request2 = new EmailVerificationRequest { Code = "87654321", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.EmailVerificationRequests.AddRange(request1, request2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _verificationRepository.RemoveAllUserEmailVerificationRequestsAsync(user.Id);

        // Assert
        var requests = await _dbContext.EmailVerificationRequests.Where(r => r.UserId == user.Id).ToListAsync();
        requests.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveAllUserEmailVerificationRequestsAsync_UserHasNoRequests_DoesNotThrowException()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _verificationRepository.RemoveAllUserEmailVerificationRequestsAsync(user.Id));
    }

    [Test]
    public async Task RemoveAllUserPasswordResetRequestsAsync_UserHasRequests_RemovesAllRequests()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { UserId = user.Id, Value = "test@example.com", State = EmailState.Active, Type = EmailType.Primary, User = user };
        _dbContext.Users.Add(user);
        _dbContext.EmailAddresses.Add(email);
        var request1 = new PasswordResetRequest { CodeHash = "hashedcode1", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        var request2 = new PasswordResetRequest { CodeHash = "hashedcode2", UserId = user.Id, EmailId = email.Id, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow, User = user, EmailAddress = email };
        _dbContext.PasswordResetRequests.AddRange(request1, request2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _verificationRepository.RemoveAllUserPasswordResetRequestsAsync(user.Id);

        // Assert
        var requests = await _dbContext.PasswordResetRequests.Where(r => r.UserId == user.Id).ToListAsync();
        requests.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveAllUserPasswordResetRequestsAsync_UserHasNoRequests_DoesNotThrowException()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _verificationRepository.RemoveAllUserPasswordResetRequestsAsync(user.Id));
    }
}
