using API.Services;
using API.Repositories.Interfaces;
using NSubstitute;
using Xunit;
using API.Contracts.Requests.Auth;
using API.Common.Helpers; // Make sure to include this
using API.Models;
using API.Common.Enums;
using Microsoft.Extensions.Configuration;
using NSubstitute.ExceptionExtensions;
using System.Threading.Tasks;
using System;
using API.Common.Utilities.Interfaces;

namespace API.UnitTests.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ISessionRepository _sessionRepository = Substitute.For<ISessionRepository>();
    private readonly IVerificationRepository _verificationRepository = Substitute.For<IVerificationRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Snowflake:MachineId", "1"},
                {"Snowflake:DatacenterId", "1"},
                {"PasswordHasher:Iterations", "10000"},
                {"PasswordHasher:SaltSize", "16"},
                {"PasswordHasher:KeySize", "32"},
                {"Email:Host", "smtp.example.com"},
                {"Email:Port", "587"},
                {"Email:EnableSsl", "true"},
                {"Email:Username", "your@example.com"},
                {"Email:Password", "password"},
                {"Email:FromEmail", "from@example.com"},
                {"Email:DisableSend", "true"}
            })
            .Build();
        
        Snowflake.Initialize(configuration);
        Hasher.Initialize(configuration);
        
        _authService = new AuthService(_userRepository, _sessionRepository, _verificationRepository, _emailSender);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            PhoneNumber = null
        };

        _userRepository.GetUserByUsername(request.Username).Returns(Task.FromResult<User?>(null));
        _userRepository.GetEmailModelByEmail(request.Email)
            .Returns(Task.FromResult<EmailAddress?>(null));
        _userRepository.GetPhoneNumberModelByPhoneNumber(Arg.Is<string>(s => s == null))
            .Returns(Task.FromResult<PhoneNumber?>(null));
        _userRepository.AddUser(Arg.Any<User>()).Returns(Task.CompletedTask);
        _userRepository.AddEmail(Arg.Any<EmailAddress>()).Returns(Task.CompletedTask);
        _verificationRepository.AddVerificationSession(Arg.Any<VerificationSession>()).Returns(Task.CompletedTask);

        // Arrange mocking EmailSender
        _emailSender.SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.True(isSuccess);
        Assert.Equal("OTP_SENT", status);
        Assert.NotNull(verificationSessionID);
        await _emailSender.Received(1).SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidEmail_ReturnsInvalidEmail()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("INVALID_EMAIL", status);
        Assert.Null(verificationSessionID);
    }

    [Fact]
    public async Task RegisterUserAsync_UsernameTaken_ReturnsUsernameTaken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _userRepository.GetUserByUsername(request.Username)
            .Returns(Task.FromResult<User?>(CreateValidUser()));

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("USERNAME_TAKEN", status);
        Assert.Null(verificationSessionID);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailTaken_ReturnsEmailTaken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _userRepository.GetUserByUsername(request.Username).Returns(Task.FromResult<User?>(null));
        _userRepository.GetEmailModelByEmail(request.Email)
            .Returns(new EmailAddress { 
                Email = "test@example.com",
                Type = EmailType.Primary,
                User = CreateValidUser(),
                State = EmailState.Verified
            });

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("EMAIL_TAKEN", status);
        Assert.Null(verificationSessionID);
    }

    [Fact]
    public async Task RegisterUserAsync_PhoneNumberTaken_ReturnsPhoneNumberTaken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            PhoneNumber = "+15551234567"
        };

        _userRepository.GetUserByUsername(request.Username).Returns(Task.FromResult<User?>(null));
        _userRepository.GetEmailModelByEmail(request.Email)
            .Returns(Task.FromResult<EmailAddress?>(null));
        _userRepository.GetPhoneNumberModelByPhoneNumber(request.PhoneNumber)
            .Returns(new PhoneNumber { 
                Phone = "+15551234567",
                User = CreateValidUser(),
                State = PhoneState.Verified,
                Type = PhoneType.Primary 
            });

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("PHONE_NUMBER_TAKEN", status);
        Assert.Null(verificationSessionID);
    }

    [Fact]
    public async Task RegisterUserAsync_PasswordTooShort_ReturnsPasswordTooShort()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Pass",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("PASSWORD_TOO_SHORT", status);
        Assert.Null(verificationSessionID);
    }

    [Fact]
    public async Task RegisterUserAsync_InternalServerError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _userRepository.GetUserByUsername(request.Username)
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("INTERNAL_SERVER_ERROR", status);
        Assert.Null(verificationSessionID);
    }

    private User CreateValidUser() => new User {
        Username = "testuser",
        FirstName = "Test",
        LastName = "User",
        PasswordHash = Convert.ToBase64String(new byte[64]),
        PasswordSalt = Convert.ToBase64String(new byte[128]),
        BirthDate = DateTime.Now.AddYears(-20)
    };
} 