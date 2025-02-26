using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Common.Enums;
using API.Common.Helpers;
using API.Configuration;
using API.Contracts.Requests.Auth;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using API.Common.Utilities.Interfaces;
using API.UnitTests.Utilities;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using JwtSettingsConfig = API.Configuration.JwtSettings;

namespace API.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<ISessionRepository> _sessionRepository;
    private readonly Mock<IVerificationRepository> _verificationRepository;
    private readonly Mock<IEmailSender> _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IOptions<PasswordHasherSettings> _passwordHasherOptions;
    private readonly IOptions<SnowflakeSettings> _snowflakeOptions;
    private readonly Mock<JwtService> _jwtService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _sessionRepository = new Mock<ISessionRepository>();
        _verificationRepository = new Mock<IVerificationRepository>();
        _emailSender = new Mock<IEmailSender>();
        
        _configuration = new ConfigurationBuilder()
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
                {"Email:DisableSend", "true"},
                {"Jwt:SecretKey", "this-is-a-32-character-long-secret-key!"},
                {"Jwt:Issuer", "test-issuer"},
                {"Jwt:Audience", "test-audience"}
            })
            .Build();
        
        // Set up IOptions for PasswordHasherSettings and SnowflakeSettings
        _passwordHasherOptions = Options.Create(new PasswordHasherSettings
        {
            Iterations = 10000,
            SaltSize = 16,
            KeySize = 32
        });
        
        _snowflakeOptions = Options.Create(new SnowflakeSettings
        {
            DatacenterId = 1,
            WorkerId = 1,
            Epoch = "2024-01-01T00:00:00Z"
        });
        
        // Initialize helpers using IOptions
        Hasher.Initialize(_passwordHasherOptions);
        Snowflake.Initialize(_snowflakeOptions);
        
        // Add mock for JwtService - use fully qualified name to avoid ambiguity
        var jwtOptions = Options.Create(new API.Configuration.JwtSettings
        {
            SecretKey = "this-is-a-32-character-long-secret-key!",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
        
        _jwtService = new Mock<JwtService>(jwtOptions);
        _jwtService.Setup(x => x.GenerateRefreshToken(It.IsAny<TokenTarget>(), It.IsAny<(long, long?, long?)>()))
            .Returns((true, "SUCCESS", "", "mock_token"));
        
        _authService = new AuthService(
            _userRepository.Object,  // Use .Object to get the actual mock instance
            _sessionRepository.Object, 
            _verificationRepository.Object, 
            _emailSender.Object,
            _jwtService.Object
        );
    }

    //---------------------------------
    //--- Register --------------------
    //---------------------------------

    [Fact]
    public async Task RegisterUserAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            PhoneNumber = null
        };

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail(request.Email))
            .ReturnsAsync((EmailAddress?)null);
        _userRepository.Setup(repo => repo.GetPhoneNumberModelByPhoneNumber(It.IsAny<string>()))
            .ReturnsAsync((PhoneNumber?)null);
        _userRepository.Setup(repo => repo.AddUser(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _userRepository.Setup(repo => repo.AddEmail(It.IsAny<EmailAddress>()))
            .Returns(Task.CompletedTask);
        _verificationRepository.Setup(repo => repo.AddVerificationSession(It.IsAny<VerificationSession>()))
            .Returns(Task.CompletedTask);
        
        // Arrange mocking EmailSender - use Moq syntax
        _emailSender.Setup(sender => sender.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.True(isSuccess);
        Assert.Equal("OTP_SENT", status);
        Assert.NotNull(verificationSessionID);
        _emailSender.Verify(sender => sender.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INVALID_EMAIL", status);
        Xunit.Assert.Null(verificationSessionID);
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

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync(TestDataFactory.CreateValidUser());

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("USERNAME_TAKEN", status);
        Xunit.Assert.Null(verificationSessionID);
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

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail(request.Email))
            .ReturnsAsync(new EmailAddress { 
                Email = "test@example.com",
                Type = EmailType.Primary,
                User = TestDataFactory.CreateValidUser(),
                State = EmailState.Verified
            });

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("EMAIL_TAKEN", status);
        Xunit.Assert.Null(verificationSessionID);
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

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail(request.Email))
            .ReturnsAsync((EmailAddress?)null);
        _userRepository.Setup(repo => repo.GetPhoneNumberModelByPhoneNumber(request.PhoneNumber))
            .ReturnsAsync(new PhoneNumber { 
                Phone = "+15551234567",
                User = TestDataFactory.CreateValidUser(),
                State = PhoneState.Verified,
                Type = PhoneType.Primary 
            });

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("PHONE_NUMBER_TAKEN", status);
        Xunit.Assert.Null(verificationSessionID);
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
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("PASSWORD_TOO_SHORT", status);
        Xunit.Assert.Null(verificationSessionID);
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

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var (isSuccess, status, message, verificationSessionID) = await _authService.RegisterUserAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INTERNAL_SERVER_ERROR", status);
        Xunit.Assert.Null(verificationSessionID);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidEmailVariations_ReturnsErrors()
    {
        // Arrange
        var invalidEmails = new[] 
        {
            "missing@domain",
            "invalid@.com",
            "@missingusername.com",
            "spaces in@email.com"
        };

        foreach(var email in invalidEmails)
        {
            var request = new RegisterRequest 
            { 
                Username = "testuser", 
                Email = email,
                Password = "ValidPass123",
                FirstName = "Test",
                LastName = "User",
                BirthDate = DateTime.Now.AddYears(-20),
                Gender = UserGender.Male
            };

            // Act
            var result = await _authService.RegisterUserAsync(request);

            // Assert
            Xunit.Assert.False(result.isSuccess);
            Xunit.Assert.Equal("INVALID_EMAIL", result.status);
        }
    }

    //---------------------------------
    //--- Verify Email ----------------
    //---------------------------------

    [Fact]
    public async Task VerifyEmailAsync_ValidSession_ReturnsSuccess()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

        var session = new VerificationSession
        {
            Id = 123,
            UserId = testUser.Id, // Match email's user ID
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiryMinutes = 10,
            User = testUser,
            Email = new EmailAddress {
                Id = 1,
                UserId = testUser.Id, // Set to same user ID
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Created,
                User = testUser
            }
        };

        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(123))
            .ReturnsAsync(session);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail("test@example.com"))
            .ReturnsAsync(session.Email);
        _userRepository.Setup(repo => repo.GetUserById(1))
            .ReturnsAsync(testUser);
        _verificationRepository.Setup(repo => repo.UpdateVerificationSession(It.IsAny<VerificationSession>()))
            .Returns(Task.CompletedTask);
        _userRepository.Setup(repo => repo.ChangeEmailState(1, EmailState.Verified))
            .Returns(Task.CompletedTask);

        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.True(isSuccess);
        Xunit.Assert.Equal("SUCCESS", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidSessionId_ReturnsNotFound()
    {
        // Arrange
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(999))
            .ReturnsAsync((VerificationSession?)null);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 999,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("NOT_FOUND", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidOtp_ReturnsInvalidOtp()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

        var session = new VerificationSession
        {
            Code = "87654321",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = testUser,
            Email = new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = testUser
            }
        };
        
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
            .ReturnsAsync(session);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "wrongotp"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INVALID_OTP", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyUsedOtp_ReturnsAlreadyUsed()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = true,
            CreatedAt = DateTime.UtcNow,
            User = testUser
        };
        
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
            .ReturnsAsync(session);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("ALREADY_USED", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredOtp_ReturnsExpired()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15),
            ExpiryMinutes = 10,
            User = testUser
        };
        
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
            .ReturnsAsync(session);
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("EXPIRED", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_EmailNotFound_ReturnsNotFound()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = testUser
        };
        
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
            .ReturnsAsync(session);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail(It.IsAny<string>()))
            .ReturnsAsync(new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = testUser
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
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("NOT_FOUND", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            User = testUser,
            Email = new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = testUser
            }
        };
        
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
            .ReturnsAsync(session);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail(It.IsAny<string>()))
            .ReturnsAsync(new EmailAddress {
                Email = "test@example.com",
                Type = EmailType.Primary,
                State = EmailState.Verified,
                User = testUser
            });
        _userRepository.Setup(repo => repo.GetUserById(1))
            .ReturnsAsync(TestDataFactory.CreateValidUser());
        
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("NOT_FOUND", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_SessionUserMismatch_ReturnsInvalidSession()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1; // Explicitly set user ID

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

        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(123))
            .ReturnsAsync(session);
        _userRepository.Setup(repo => repo.GetEmailModelByEmail("test@example.com"))
            .ReturnsAsync(session.Email);
        _userRepository.Setup(repo => repo.GetUserById(1))
            .ReturnsAsync(sessionUser);

        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 123,
            Email = "test@example.com",
            OTP = "12345678"
        };

        // Act
        var (isSuccess, status, _, _) = await _authService.VerifyEmailAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INVALID_SESSION", status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InternalError_ReturnsInternalError()
    {
        // Arrange
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
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
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INTERNAL_SERVER_ERROR", status);
    }

    //---------------------------------
    //--- Login -----------------------
    //---------------------------------

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var validUser = TestDataFactory.CreateValidUser();
        var (hash, salt) = Hasher.Hash("Password123");
        validUser.PasswordHash = hash;
        validUser.PasswordSalt = salt;
        validUser.State = UserState.Active;

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123"
        };

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync(validUser);
        _sessionRepository.Setup(repo => repo.AddSession(It.IsAny<Session>()))
            .Returns(Task.CompletedTask);
        _sessionRepository.Setup(repo => repo.AddToken(It.IsAny<Token>()))
            .Returns(Task.CompletedTask);
        
        var mockJwtService = new Mock<JwtService>(Mock.Of<IOptions<API.Configuration.JwtSettings>>());
        mockJwtService.Setup(x => x.GenerateRefreshToken(It.IsAny<TokenTarget>(), It.IsAny<(long, long?, long?)>()))
            .Returns((true, "SUCCESS", "", "mock_token"));
        
        var authService = new AuthService(
            _userRepository.Object, 
            _sessionRepository.Object, 
            _verificationRepository.Object, 
            _emailSender.Object,
            mockJwtService.Object
        );

        // Act
        var (isSuccess, status, _, accessToken, refreshToken) = await authService.LoginAsync(request);

        // Assert
        Xunit.Assert.True(isSuccess);
        Xunit.Assert.Equal("SUCCESS", status);
    }

    [Fact]
    public async Task LoginAsync_AccountLocked_ReturnsAccountLocked()
    {
        // Arrange
        var validUser = TestDataFactory.CreateValidUser();
        validUser.State = UserState.Suspended;  // Changed from Locked
        var (hash, salt) = Hasher.Hash("Password123");
        validUser.PasswordHash = hash;
        validUser.PasswordSalt = salt;

        var request = new LoginRequest
        {
            Username = "suspendeduser",
            Password = "Password123"
        };

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync(validUser);

        // Act
        var (isSuccess, status, _, _, _) = await _authService.LoginAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("ACCOUNT_LOCKED", status);
    }

    [Fact]
    public async Task LoginAsync_UserInactive_ReturnsUserInactive()
    {
        // Arrange
        var validUser = TestDataFactory.CreateValidUser();
        validUser.State = UserState.PendingVerification;  // Changed from Inactive
        var (hash, salt) = Hasher.Hash("Password123");
        validUser.PasswordHash = hash;
        validUser.PasswordSalt = salt;

        var request = new LoginRequest
        {
            Username = "pendinguser",
            Password = "Password123"
        };

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync(validUser);

        // Act
        var (isSuccess, status, _, _, _) = await _authService.LoginAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("USER_INACTIVE", status);
    }

    [Fact]
    public async Task LoginAsync_EmptyUsername_ReturnsInvalidRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "",
            Password = "Password123"
        };

        // Act
        var (isSuccess, status, _, _, _) = await _authService.LoginAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INVALID_REQUEST", status);
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ReturnsInvalidRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        var (isSuccess, status, _, _, _) = await _authService.LoginAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INVALID_REQUEST", status);
    }

    [Fact]
    public async Task LoginAsync_DeletedUser_ReturnsAccountLocked()
    {
        // Arrange
        var validUser = TestDataFactory.CreateValidUser();
        validUser.State = UserState.Deleted;
        var (hash, salt) = Hasher.Hash("Password123");
        validUser.PasswordHash = hash;
        validUser.PasswordSalt = salt;

        var request = new LoginRequest
        {
            Username = "deleteduser",
            Password = "Password123"
        };

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync(validUser);

        // Act
        var (isSuccess, status, _, _, _) = await _authService.LoginAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("ACCOUNT_DELETED", status);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentialsButTokenGenerationFails_ReturnsInternalError()
    {
        // Arrange
        var validUser = TestDataFactory.CreateValidUser();
        validUser.State = UserState.Active;
        var (hash, salt) = Hasher.Hash("Password123");
        validUser.PasswordHash = hash;
        validUser.PasswordSalt = salt;

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123"
        };

        _userRepository.Setup(repo => repo.GetUserByUsername(request.Username))
            .ReturnsAsync(validUser);
        
        // Add mock JwtService
        var mockJwtService = new Mock<JwtService>(Mock.Of<IOptions<API.Configuration.JwtSettings>>());
        mockJwtService.Setup(x => x.GenerateRefreshToken(It.IsAny<TokenTarget>(), It.IsAny<(long, long?, long?)>()))
            .Returns((true, "SUCCESS", "", "mock_token"));

        // Create AuthService with all dependencies
        var authService = new AuthService(
            _userRepository.Object, 
            _sessionRepository.Object, 
            _verificationRepository.Object, 
            _emailSender.Object,
            mockJwtService.Object
        );
        
        // Force token generation failure
        _sessionRepository.Setup(repo => repo.AddToken(It.IsAny<Token>()))
            .ThrowsAsync(new Exception("Token storage failed"));

        // Act
        var (isSuccess, status, _, _, _) = await authService.LoginAsync(request);

        // Assert
        Xunit.Assert.False(isSuccess);
        Xunit.Assert.Equal("INTERNAL_SERVER_ERROR", status);
    }

    //---------------------------------
    //--- Concurrent Requests ---------
    //---------------------------------

    [Fact] 
    public async Task VerifyEmailAsync_ConcurrentRequests_HandlesLocking()
    {
        // Arrange
        var testUser = TestDataFactory.CreateValidUser();
        testUser.Id = 1;

        var session = new VerificationSession
        {
            Code = "12345678",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            ExpiryMinutes = 30,
            User = testUser,
            UserId = testUser.Id,
            Email = new EmailAddress 
            { 
                Email = "test@example.com",
                UserId = testUser.Id,
                User = testUser,
                Type = EmailType.Primary,
                State = EmailState.Created
            }
        };

        // Update mock to use same session instance
        _verificationRepository.Setup(repo => repo.GetVerificationSessionById(It.IsAny<long>()))
            .ReturnsAsync(session); // Use ReturnsAsync instead of Returns
        _verificationRepository.Setup(repo => repo.UpdateVerificationSession(It.IsAny<VerificationSession>()))
            .Returns(Task.CompletedTask)
            .Callback<VerificationSession>(vs => session.IsUsed = vs.IsUsed); // Use Callback instead of AndDoes

        _userRepository.Setup(repo => repo.GetEmailModelByEmail(It.IsAny<string>()))
            .ReturnsAsync(session.Email);
        _userRepository.Setup(repo => repo.GetUserById(It.IsAny<long>()))
            .ReturnsAsync(testUser);
        _userRepository.Setup(repo => repo.ChangeEmailState(It.IsAny<long>(), It.IsAny<EmailState>()))
            .Returns(Task.CompletedTask);

        var request = new VerifyEmailRequest 
        { 
            VerificationSessionID = 1,
            OTP = "12345678",
            Email = "test@example.com"
        };

        // Act
        var task1 = _authService.VerifyEmailAsync(request);
        var task2 = _authService.VerifyEmailAsync(request);
        var results = await Task.WhenAll(task1, task2);

        // Assert
        Xunit.Assert.Equal(1, results.Count(r => r.isSuccess));
        Xunit.Assert.Contains(results, r => r.status == "ALREADY_USED");
    }
}