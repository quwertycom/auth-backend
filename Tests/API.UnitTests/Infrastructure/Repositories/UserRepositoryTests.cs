using NUnit.Framework;
using API.Infrastructure.Database.Repositories;
using API.Infrastructure.Database;
using NSubstitute;
using FluentAssertions;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;
using Microsoft.EntityFrameworkCore;
using Assert = NUnit.Framework.Assert;
using System;

namespace API.UnitTests.Infrastructure.Repositories;

public class UserRepositoryTests : TestBase
{
    private AuthDbContext _dbContext = null!;
    private UserRepository _userRepository = null!;

    [SetUp]
    public override void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new AuthDbContext(dbContextOptions);
        _userRepository = new UserRepository(_dbContext);

        // Ensure database is created and cleared for each test
        _dbContext.Database.EnsureCreated();
        _dbContext.Users.RemoveRange(_dbContext.Users);
        _dbContext.EmailAddresses.RemoveRange(_dbContext.EmailAddresses);
        _dbContext.PhoneNumbers.RemoveRange(_dbContext.PhoneNumbers);
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
    public async Task AddUserAsync_ValidUser_AddsUserToDatabase()
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
        await _userRepository.AddUserAsync(user);

        // Assert
        var retrievedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetUserByUsernameAsync_UserExists_ReturnsUser()
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
        var retrievedUser = await _userRepository.GetUserByUsernameAsync("testuser");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetUserByUsernameAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange (No user added to the database)

        // Act
        var retrievedUser = await _userRepository.GetUserByUsernameAsync("nonexistentuser");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Test]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
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
        var retrievedUser = await _userRepository.GetUserByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Id.Should().Be(user.Id);
    }

    [Test]
    public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange (No user added with specific ID)
        long nonExistentUserId = 999;

        // Act
        var retrievedUser = await _userRepository.GetUserByIdAsync(nonExistentUserId);

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Test]
    public async Task UpdateUserStateAsync_UserExists_UpdatesUserState()
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
        await _userRepository.UpdateUserStateAsync(user.Id, UserState.Active);

        // Assert
        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser?.State.Should().Be(UserState.Active);
    }

    [Test]
    public void UpdateUserStateAsync_UserDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentUserId = 999;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.UpdateUserStateAsync(nonExistentUserId, UserState.Active));
    }

    [Test]
    public async Task AddEmailAsync_ValidEmail_AddsEmailToDatabase()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var email = new EmailAddress { Value = "test@example.com", UserId = user.Id, Type = EmailType.Primary, State = EmailState.PendingVerification, User = user };

        // Act
        await _userRepository.AddEmailAsync(email);

        // Assert
        var retrievedEmail = await _dbContext.EmailAddresses.FirstOrDefaultAsync(e => e.Value == "test@example.com");
        retrievedEmail.Should().NotBeNull();
        retrievedEmail?.Value.Should().Be("test@example.com");
    }
    [Test]
    public async Task AddPhoneNumberAsync_ValidPhoneNumber_AddsPhoneNumberToDatabase()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var phoneNumber = new PhoneNumber { Value = "+1234567890", UserId = user.Id, Type = PhoneType.Primary, State = PhoneState.PendingVerification, User = user };

        // Act
        await _userRepository.AddPhoneNumberAsync(phoneNumber);

        // Assert
        var retrievedPhoneNumber = await _dbContext.PhoneNumbers.FirstOrDefaultAsync(p => p.Value == "+1234567890");
        retrievedPhoneNumber.Should().NotBeNull();
        retrievedPhoneNumber?.Value.Should().Be("+1234567890");
    }

    [Test]
    public async Task GetUserByEmailAsync_EmailExists_ReturnsUser()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var email = new EmailAddress { Value = "test@example.com", UserId = user.Id, Type = EmailType.Primary, State = EmailState.PendingVerification, User = user };
        user.EmailAddresses.Add(email);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _userRepository.GetUserByEmailAsync("test@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetUserByEmailAsync_EmailDoesNotExist_ReturnsNull()
    {
        // Arrange (No user with the email added)
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var email = new EmailAddress { Value = "nonexistent@example.com", UserId = user.Id, Type = EmailType.Primary, State = EmailState.PendingVerification, User = user };

        // Act
        var retrievedUser = await _userRepository.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Test]
    public async Task GetUserPrimaryEmailAddressAsync_PrimaryEmailExists_ReturnsEmailAddress()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var primaryEmail = new EmailAddress { Value = "primary@example.com", UserId = user.Id, Type = EmailType.Primary, State = EmailState.PendingVerification, User = user };
        user.EmailAddresses.Add(primaryEmail);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedEmail = await _userRepository.GetUserPrimaryEmailAddressAsync(user.Id);

        // Assert
        retrievedEmail.Should().NotBeNull();
        retrievedEmail?.Value.Should().Be("primary@example.com");
        retrievedEmail?.Type.Should().Be(EmailType.Primary);
    }

    [Test]
    public async Task GetUserPrimaryEmailAddressAsync_PrimaryEmailDoesNotExist_ReturnsNull()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var otherEmail = new EmailAddress { Value = "other@example.com", UserId = user.Id, Type = EmailType.Other, State = EmailState.PendingVerification, User = user };
        user.EmailAddresses.Add(otherEmail);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedEmail = await _userRepository.GetUserPrimaryEmailAddressAsync(user.Id);

        // Assert
        retrievedEmail.Should().BeNull();
    }

    [Test]
    public async Task GetEmailAdressByIdAsync_EmailExists_ReturnsEmailAddress()
    {
        // Arrange
        var email = new EmailAddress { Value = "test@example.com", UserId = 1, Type = EmailType.Primary, State = EmailState.PendingVerification, User = new User() { Id = 1, Username = "temp", FirstName = "temp", LastName = "temp", PasswordHash = "temp", PasswordSalt = "temp", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification } };
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedEmail = await _userRepository.GetEmailAdressByIdAsync(email.Id);

        // Assert
        retrievedEmail.Should().NotBeNull();
        retrievedEmail?.Id.Should().Be(email.Id);
    }

    [Test]
    public async Task GetEmailAdressByEmailStringAsync_EmailExists_ReturnsEmailAddress()
    {
        // Arrange
        var email = new EmailAddress { Value = "test@example.com", UserId = 1, Type = EmailType.Primary, State = EmailState.PendingVerification, User = new User() { Id = 1, Username = "temp", FirstName = "temp", LastName = "temp", PasswordHash = "temp", PasswordSalt = "temp", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification } };
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedEmail = await _userRepository.GetEmailAdressByEmailStringAsync("test@example.com");

        // Assert
        retrievedEmail.Should().NotBeNull();
        retrievedEmail?.Value.Should().Be("test@example.com");
    }

    [Test]
    public async Task EmailAdressExistsAsync_EmailExists_ReturnsTrue()
    {
        // Arrange
        var email = new EmailAddress { Value = "test@example.com", UserId = 1, Type = EmailType.Primary, State = EmailState.PendingVerification, User = new User() { Id = 1, Username = "temp", FirstName = "temp", LastName = "temp", PasswordHash = "temp", PasswordSalt = "temp", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification } };
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();

        // Act
        var exists = await _userRepository.EmailAdressExistsAsync("test@example.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task EmailAdressExistsAsync_EmailDoesNotExist_ReturnsFalse()
    {
        // Arrange (No email with specific string)

        // Act
        var exists = await _userRepository.EmailAdressExistsAsync("nonexistent@example.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task UsernameExistsAsync_UsernameExists_ReturnsTrue()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var exists = await _userRepository.UsernameExistsAsync("testuser");

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task UsernameExistsAsync_UsernameDoesNotExist_ReturnsFalse()
    {
        // Arrange (No user with specific username)

        // Act
        var exists = await _userRepository.UsernameExistsAsync("nonexistentuser");

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task PhoneNumberExistsAsync_PhoneNumberExists_ReturnsTrue()
    {
        // Arrange
        var phoneNumber = new PhoneNumber { Value = "+1234567890", UserId = 1, Type = PhoneType.Primary, State = PhoneState.PendingVerification, User = new User() { Id = 1, Username = "temp", FirstName = "temp", LastName = "temp", PasswordHash = "temp", PasswordSalt = "temp", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification } };
        _dbContext.PhoneNumbers.Add(phoneNumber);
        await _dbContext.SaveChangesAsync();

        // Act
        var exists = await _userRepository.PhoneNumberExistsAsync("+1234567890");

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task PhoneNumberExistsAsync_PhoneNumberDoesNotExist_ReturnsFalse()
    {
        // Arrange (No phone number with specific string)

        // Act
        var exists = await _userRepository.PhoneNumberExistsAsync("nonexistentnumber");

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task UpdateEmailStateAsync_EmailExists_UpdatesEmailState()
    {
        // Arrange
        var email = new EmailAddress { Value = "test@example.com", UserId = 1, Type = EmailType.Primary, State = EmailState.PendingVerification, User = new User() { Id = 1, Username = "temp", FirstName = "temp", LastName = "temp", PasswordHash = "temp", PasswordSalt = "temp", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification } };
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();

        // Act
        await _userRepository.UpdateEmailStateAsync(email.Id, EmailState.Active);

        // Assert
        var updatedEmail = await _dbContext.EmailAddresses.FindAsync(email.Id);
        updatedEmail.Should().NotBeNull();
        updatedEmail?.State.Should().Be(EmailState.Active);
    }

    [Test]
    public void UpdateEmailStateAsync_EmailDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentEmailId = 999;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.UpdateEmailStateAsync(nonExistentEmailId, EmailState.Active));
    }

    [Test]
    public async Task ChangeUserPrimaryEmailAddressAsync_UserExists_ChangesPrimaryEmail()
    {
        // Arrange
        var user = new User { Username = "testuser", FirstName = "Test", LastName = "User", PasswordHash = "hash", PasswordSalt = "salt", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification };
        var primaryEmail = new EmailAddress { Value = "primary@example.com", UserId = user.Id, Type = EmailType.Primary, State = EmailState.Active, User = user };
        var newPrimaryEmail = new EmailAddress { Value = "newprimary@example.com", UserId = user.Id, Type = EmailType.Other, State = EmailState.Active,  User = user }; // Initially Other
        user.EmailAddresses.Add(primaryEmail);
        user.EmailAddresses.Add(newPrimaryEmail);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _userRepository.ChangeUserPrimaryEmailAddressAsync(user.Id, newPrimaryEmail.Id);
        var updatedPrimaryEmail = await _userRepository.GetUserPrimaryEmailAddressAsync(user.Id);
        var updatedNewPrimaryEmail = await _dbContext.EmailAddresses.FindAsync(newPrimaryEmail.Id);


        // Assert
        updatedPrimaryEmail.Should().NotBeNull();
        updatedPrimaryEmail?.Value.Should().Be("newprimary@example.com");
        updatedPrimaryEmail?.Type.Should().Be(EmailType.Primary);
        updatedNewPrimaryEmail?.Type.Should().Be(EmailType.Primary);
    }

     [Test]
    public void ChangeUserPrimaryEmailAddressAsync_UserDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentUserId = 999;
        long emailId = 123;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.ChangeUserPrimaryEmailAddressAsync(nonExistentUserId, emailId));
    }

    [Test]
    public async Task UpdateUserPasswordAsync_UserExists_UpdatesPassword()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "oldHash",
            PasswordSalt = "oldSalt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _userRepository.UpdateUserPasswordAsync(user.Id, "newHash", "newSalt");

        // Assert
        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser?.PasswordHash.Should().Be("newHash");
        updatedUser?.PasswordSalt.Should().Be("newSalt");
    }

    [Test]
    public void UpdateUserPasswordAsync_UserDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentUserId = 999;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.UpdateUserPasswordAsync(nonExistentUserId, "newHash", "newSalt"));
    }

    [Test]
    public async Task UpdateUserLastLoginAsync_UserExists_UpdatesLastLoginAt()
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
            State = UserState.PendingVerification,
            LastLoginAt = null // Initially null
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _userRepository.UpdateUserLastLoginAsync(user.Id);

        // Assert
        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser?.LastLoginAt.Should().NotBeNull(); // Should be updated to not null
        updatedUser?.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // Verify close to current time
    }

    [Test]
    public void UpdateUserLastLoginAsync_UserDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentUserId = 999;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.UpdateUserLastLoginAsync(nonExistentUserId));
    }

    [Test]
    public async Task RemoveEmailAddressAsync_EmailExists_RemovesEmailFromDatabase()
    {
        // Arrange
        var email = new EmailAddress { Value = "test@example.com", UserId = 1, Type = EmailType.Primary, State = EmailState.PendingVerification, User = new User() { Id = 1, Username = "temp", FirstName = "temp", LastName = "temp", PasswordHash = "temp", PasswordSalt = "temp", BirthDate = DateTime.Now.AddYears(-20), Gender = UserGender.Male, State = UserState.PendingVerification } };
        _dbContext.EmailAddresses.Add(email);
        await _dbContext.SaveChangesAsync();

        // Act
        await _userRepository.RemoveEmailAddressAsync(email.Id);

        // Assert
        var retrievedEmail = await _dbContext.EmailAddresses.FindAsync(email.Id);
        retrievedEmail.Should().BeNull(); // Should be removed
    }

    [Test]
    public void RemoveEmailAddressAsync_EmailDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentEmailId = 999;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.RemoveEmailAddressAsync(nonExistentEmailId));
    }

    [Test]
    public async Task RemoveUserAsync_UserExists_RemovesUserFromDatabase()
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
        await _userRepository.RemoveUserAsync(user.Id);

        // Assert
        var retrievedUser = await _dbContext.Users.FindAsync(user.Id);
        retrievedUser.Should().BeNull(); // Should be removed
    }

    [Test]
    public void RemoveUserAsync_UserDoesNotExist_ThrowsException()
    {
        // Arrange
        long nonExistentUserId = 999;

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.RemoveUserAsync(nonExistentUserId));
    }
}
