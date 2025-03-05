using API.Features.Authentication.EmailVerification.Services;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Email;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;
using API.Shared.Utilities;

namespace API.UnitTests.Features.Authentication.EmailVerification;

public class EmailVerificationServiceTests : TestBase
{
    private EmailVerificationService? _emailVerificationService;
    private IUserRepository? _mockUserRepository;
    private IVerificationRepository? _mockVerificationRepository;
    private IRandomGenerator? _mockRandomGenerator;
    private IEmailSender? _mockEmailSender;

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockVerificationRepository = Substitute.For<IVerificationRepository>();
        _mockRandomGenerator = Substitute.For<IRandomGenerator>();
        _mockEmailSender = Substitute.For<IEmailSender>();

        _emailVerificationService = new EmailVerificationService(
            _mockUserRepository,
            _mockVerificationRepository,
            _mockRandomGenerator,
            _mockEmailSender
        );

        // Default mock setups for success case
        _mockRandomGenerator.GenerateNumberCode(8).Returns("12345678");
        _mockEmailSender.SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
    }

    #region Helper Methods

    private User CreateMockUser(UserState state = UserState.PendingVerification)
    {
        return new User
        {
            Id = Snowflake.Generate(),
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedpassword",
            PasswordSalt = "salt",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male,
            State = state
        };
    }

    private EmailAddress CreateMockEmailAddress(string email, EmailState state, User? user = null)
    {
        var mockUser = user ?? CreateMockUser();
        return new EmailAddress
        {
            Id = Snowflake.Generate(),
            UserId = mockUser.Id,
            Value = email,
            State = state,
            Type = EmailType.Primary,
            User = mockUser
        };
    }

    private EmailVerificationRequest CreateMockVerificationRequest(
        string requestId, 
        string code, 
        EmailAddress emailAddress,
        User user,
        bool isUsed = false,
        bool isExpired = false)
    {
        return new EmailVerificationRequest
        {
            Id = long.Parse(requestId),
            Code = code,
            User = user,
            EmailAddress = emailAddress,
            ExpiresAt = isExpired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(5),
            IsUsed = isUsed
        };
    }

    #endregion

    #region RequestEmailVerificationAsync Tests

    [Test]
    public async Task RequestEmailVerificationAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(emailAddress, EmailState.PendingVerification, mockUser);

        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(mockUser));

        // Act
        var result = await _emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.RequestId.Should().NotBeNullOrEmpty();

        await _mockVerificationRepository!.Received(1).AddEmailVerificationRequestAsync(Arg.Is<EmailVerificationRequest>(req =>
            req.Code == "12345678" &&
            req.EmailAddress == mockEmailAddressEntity &&
            req.User == mockUser &&
            req.ExpiresAt > DateTime.UtcNow &&
            req.CreatedAt <= DateTime.UtcNow
        ));
        await _mockEmailSender!.Received(1).SendOtpEmailAsync(emailAddress, "12345678", "Test", "en");
    }

    [Test]
    public async Task RequestEmailVerificationAsync_EmailNotFound_ReturnsEmailNotFoundResult()
    {
        // Arrange
        string emailAddress = "nonexistent@example.com";
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(null));

        // Act
        var result = await _emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_NOT_FOUND");
        result.RequestId.Should().BeNull();

        await _mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await _mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestEmailVerificationAsync_UserNotFound_ReturnsUserNotFoundResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(emailAddress, EmailState.PendingVerification, mockUser);
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(null));

        // Act
        var result = await _emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("USER_NOT_FOUND");
        result.RequestId.Should().BeNull();

        await _mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await _mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestEmailVerificationAsync_EmailNotPendingVerification_ReturnsEmailNotVerifiedResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(emailAddress, EmailState.Active, mockUser);

        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(mockUser));

        // Act
        var result = await _emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_NOT_VERIFIED");
        result.RequestId.Should().BeNull();

        await _mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await _mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestEmailVerificationAsync_EmailSendingFails_ReturnsEmailSendingFailedResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(emailAddress, EmailState.PendingVerification, mockUser);

        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(mockUser));
        _mockEmailSender!.SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        var result = await _emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_SENDING_FAILED");
        result.RequestId.Should().BeNull();

        await _mockVerificationRepository!.Received(1).AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await _mockEmailSender!.Received(1).SendOtpEmailAsync(emailAddress, "12345678", "Test", "en");
    }

    [Test]
    public async Task RequestEmailVerificationAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress)
            .Returns(Task.FromException<EmailAddress?>(new Exception("Database error")));

        // Act
        var result = await _emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.RequestId.Should().BeNull();

        await _mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await _mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
    
    #endregion
    
    #region GetRequestStatusAsync Tests
    
    [Test]
    public async Task GetRequestStatusAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        var mockUser = CreateMockUser();
        var emailAddressEntity = CreateMockEmailAddress(email, EmailState.PendingVerification, mockUser);
        var verificationRequest = CreateMockVerificationRequest(requestId, "12345678", emailAddressEntity, mockUser);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));
            
        _mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.IsValid.Should().BeTrue();
    }
    
    [Test]
    public async Task GetRequestStatusAsync_RequestNotFound_ReturnsRequestNotFoundResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(null));
            
        // Act
        var result = await _emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_NOT_FOUND");
        result.HttpStatusCode.Should().Be(404);
    }
    
    [Test]
    public async Task GetRequestStatusAsync_EmailMismatch_ReturnsEmailMismatchResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        string differentEmail = "different@example.com";
        
        var mockUser = CreateMockUser();
        var emailAddressEntity = CreateMockEmailAddress(differentEmail, EmailState.PendingVerification, mockUser);
        var verificationRequest = CreateMockVerificationRequest(requestId, "12345678", emailAddressEntity, mockUser);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));
            
        _mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_MISMATCH");
    }
    
    [Test]
    public async Task GetRequestStatusAsync_RequestUsed_ReturnsRequestUsedResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        
        var mockUser = CreateMockUser();
        var emailAddressEntity = CreateMockEmailAddress(email, EmailState.PendingVerification, mockUser);
        var verificationRequest = CreateMockVerificationRequest(requestId, "12345678", emailAddressEntity, mockUser, isUsed: true);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));
            
        _mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_USED");
    }
    
    [Test]
    public async Task GetRequestStatusAsync_RequestExpired_ReturnsRequestExpiredResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        
        var mockUser = CreateMockUser();
        var emailAddressEntity = CreateMockEmailAddress(email, EmailState.PendingVerification, mockUser);
        var verificationRequest = CreateMockVerificationRequest(requestId, "12345678", emailAddressEntity, mockUser, isExpired: true);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));
            
        _mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_EXPIRED");
        result.HttpStatusCode.Should().Be(410);
    }
    
    [Test]
    public async Task GetRequestStatusAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromException<EmailVerificationRequest?>(new Exception("Database error")));
            
        // Act
        var result = await _emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.HttpStatusCode.Should().Be(500);
    }
    
    #endregion
    
    #region VerifyEmailAsync Tests
    
    [Test]
    public async Task VerifyEmailAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        
        var userId = Snowflake.Generate();
        var emailId = Snowflake.Generate();
        
        var user = CreateMockUser();
        user.Id = userId;
        
        var emailAddress = CreateMockEmailAddress("test@example.com", EmailState.PendingVerification, user);
        emailAddress.Id = emailId;
        
        var request = CreateMockVerificationRequest(requestId, code, emailAddress, user);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));
            
        // Act
        var result = await _emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.RequestId.Should().Be(requestId);
        
        await _mockVerificationRepository!.Received(1).MarkEmailVerificationRequestAsUsedAsync(long.Parse(requestId));
        await _mockUserRepository!.Received(1).UpdateUserStateAsync(userId, UserState.Active);
        await _mockUserRepository!.Received(1).UpdateEmailStateAsync(emailId, EmailState.Active);
    }
    
    [Test]
    public async Task VerifyEmailAsync_RequestNotFound_ReturnsRequestNotFoundResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(null));
            
        // Act
        var result = await _emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_NOT_FOUND");
        result.HttpStatusCode.Should().Be(404);
        
        await _mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await _mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await _mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }
    
    [Test]
    public async Task VerifyEmailAsync_RequestUsed_ReturnsRequestUsedResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        
        var userId = Snowflake.Generate();
        var emailId = Snowflake.Generate();
        
        var user = CreateMockUser();
        user.Id = userId;
        
        var emailAddress = CreateMockEmailAddress("test@example.com", EmailState.PendingVerification, user);
        emailAddress.Id = emailId;
        
        var request = CreateMockVerificationRequest(requestId, code, emailAddress, user, isUsed: true);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));
            
        // Act
        var result = await _emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_USED");
        result.HttpStatusCode.Should().Be(400);
        
        await _mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await _mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await _mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }
    
    [Test]
    public async Task VerifyEmailAsync_RequestExpired_ReturnsRequestExpiredResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        
        var userId = Snowflake.Generate();
        var emailId = Snowflake.Generate();
        
        var user = CreateMockUser();
        user.Id = userId;
        
        var emailAddress = CreateMockEmailAddress("test@example.com", EmailState.PendingVerification, user);
        emailAddress.Id = emailId;
        
        var request = CreateMockVerificationRequest(requestId, code, emailAddress, user, isExpired: true);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));
            
        // Act
        var result = await _emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_EXPIRED");
        result.HttpStatusCode.Should().Be(410);
        
        await _mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await _mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await _mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }
    
    [Test]
    public async Task VerifyEmailAsync_CodeMismatch_ReturnsCodeMismatchResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        string incorrectCode = "87654321";
        
        var userId = Snowflake.Generate();
        var emailId = Snowflake.Generate();
        
        var user = CreateMockUser();
        user.Id = userId;
        
        var emailAddress = CreateMockEmailAddress("test@example.com", EmailState.PendingVerification, user);
        emailAddress.Id = emailId;
        
        var request = CreateMockVerificationRequest(requestId, code, emailAddress, user);
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));
            
        // Act
        var result = await _emailVerificationService!.VerifyEmailAsync(requestId, incorrectCode, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("CODE_MISMATCH");
        result.HttpStatusCode.Should().Be(400);
        
        await _mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await _mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await _mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }
    
    [Test]
    public async Task VerifyEmailAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        
        _mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromException<EmailVerificationRequest?>(new Exception("Database error")));
            
        // Act
        var result = await _emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.HttpStatusCode.Should().Be(500);
        
        await _mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await _mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await _mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }
    
    #endregion
    
    #region RequestNewCodeAsync Tests
    
    [Test]
    public async Task RequestNewCodeAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string email = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(email, EmailState.PendingVerification, mockUser);
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
            
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId)
            .Returns(Task.FromResult<User?>(mockEmailAddressEntity.User));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.NewRequestId.Should().NotBeNullOrEmpty();
        result.HttpStatusCode.Should().Be(200);
    }
    
    [Test]
    public async Task RequestNewCodeAsync_EmailNotFound_ReturnsEmailNotFoundResult()
    {
        // Arrange
        string email = "nonexistent@example.com";
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(null));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_NOT_FOUND");
        result.HttpStatusCode.Should().Be(404);
    }
    
    [Test]
    public async Task RequestNewCodeAsync_EmailAlreadyVerified_ReturnsEmailAlreadyVerifiedResult()
    {
        // Arrange
        string email = "verified@example.com";
        var mockUser = CreateMockUser(UserState.Active);
        var mockEmailAddressEntity = CreateMockEmailAddress(email, EmailState.Active, mockUser);
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_ALREADY_VERIFIED");
        result.HttpStatusCode.Should().Be(400);
    }
    
    [Test]
    public async Task RequestNewCodeAsync_EmailBlacklisted_ReturnsEmailBlacklistedResult()
    {
        // Arrange
        string email = "blacklisted@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(email, EmailState.Blacklisted, mockUser);
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_BLACKLISTED");
        result.HttpStatusCode.Should().Be(400);
    }
    
    [Test]
    public async Task RequestNewCodeAsync_EmailDisabled_ReturnsEmailDisabledResult()
    {
        // Arrange
        string email = "disabled@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(email, EmailState.Disabled, mockUser);
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_DISABLED");
        result.HttpStatusCode.Should().Be(400);
    }
    
    [Test]
    public async Task RequestNewCodeAsync_EmailDeleted_ReturnsEmailDisabledResult()
    {
        // Arrange
        string email = "deleted@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddressEntity = CreateMockEmailAddress(email, EmailState.Deleted, mockUser);
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_DISABLED");
        result.HttpStatusCode.Should().Be(400);
    }
    
    [Test]
    public async Task RequestNewCodeAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        string email = "test@example.com";
        
        _mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromException<EmailAddress?>(new Exception("Database error")));
            
        // Act
        var result = await _emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.HttpStatusCode.Should().Be(500);
    }
    
    #endregion
}
