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
using API.UnitTests.Utilities;
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
            .Returns(Task.FromResult<User?>(TestDataFactory.CreateValidUser()));

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
                User = TestDataFactory.CreateValidUser(),
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
                User = TestDataFactory.CreateValidUser(),
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

    [Fact]
    public async Task VerifyEmailAsync_ValidSession_ReturnsSuccess()
    {
        // Arrange
        var validUser = TestDataFactory.CreateValidUser();
        var session = new VerificationSession
        {
            Id = 123,
            UserId = 1,
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiryMinutes = 10,
            User = validUser,
            Email = new EmailAddress {
                Id = 1,
                UserId = 1,
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Created,
                User = validUser
            }
        };

        _verificationRepository.GetVerificationSessionById(123).Returns(session);
        _userRepository.GetEmailModelByEmail("test@example.com").Returns(session.Email);
        _userRepository.GetUserById(1).Returns(validUser);
        _verificationRepository.UpdateVerificationSession(Arg.Any<VerificationSession>()).Returns(Task.CompletedTask);
        _userRepository.ChangeEmailState(1, EmailState.Verified).Returns(Task.CompletedTask);

        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.True(isSuccess);
        Assert.Equal("SUCCESS", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidSessionId_ReturnsNotFound()
    {
        // Arrange
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>()).Returns((VerificationSession?)null);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 999,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("NOT_FOUND", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidOtp_ReturnsInvalidOtp()
    {
        // Arrange
        var session = new VerificationSession
        {
            Code = "87654321",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = TestDataFactory.CreateValidUser(),
            Email = new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = TestDataFactory.CreateValidUser()
            }
        };
        
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>()).Returns(session);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "wrongotp"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("INVALID_OTP", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyUsedOtp_ReturnsAlreadyUsed()
    {
        // Arrange
        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = true,
            CreatedAt = DateTime.UtcNow,
            User = TestDataFactory.CreateValidUser()
        };
        
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>()).Returns(session);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("ALREADY_USED", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredOtp_ReturnsExpired()
    {
        // Arrange
        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15),
            ExpiryMinutes = 10,
            User = TestDataFactory.CreateValidUser()
        };
        
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>()).Returns(session);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("EXPIRED", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_EmailNotFound_ReturnsNotFound()
    {
        // Arrange
        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = TestDataFactory.CreateValidUser()
        };
        
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>()).Returns(session);
        _userRepository.GetEmailModelByEmail(Arg.Any<string>())
            .Returns(new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = TestDataFactory.CreateValidUser()
            });
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("NOT_FOUND", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = TestDataFactory.CreateValidUser(),
            Email = new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = TestDataFactory.CreateValidUser()
            }
        };
        
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>()).Returns(session);
        _userRepository.GetEmailModelByEmail(Arg.Any<string>())
            .Returns(new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = TestDataFactory.CreateValidUser()
            });
        _userRepository.GetUserById(1).Returns(TestDataFactory.CreateValidUser());
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("NOT_FOUND", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_SessionUserMismatch_ReturnsInvalidSession()
    {
        // Arrange
        var sessionUser = TestDataFactory.CreateValidUser();
        var emailUser = new User { 
            Id = 2,
            Username = "otheruser",
            FirstName = "Other",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            BirthDate = DateTime.UtcNow.AddYears(-25)
        };

        var session = new VerificationSession
        {
            Id = 123,
            UserId = 1,
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = sessionUser,
            Email = new EmailAddress {
                Id = 1,
                UserId = 2,
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Created,
                User = emailUser
            }
        };

        _verificationRepository.GetVerificationSessionById(123).Returns(session);
        _userRepository.GetEmailModelByEmail("test@example.com").Returns(session.Email);
        _userRepository.GetUserById(1).Returns(sessionUser);

        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("INVALID_SESSION", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InternalError_ReturnsInternalError()
    {
        // Arrange
        _verificationRepository.GetVerificationSessionById(Arg.Any<long>())
            .ThrowsAsync(new Exception("Database error"));
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("INTERNAL_SERVER_ERROR", status);
    }
} 