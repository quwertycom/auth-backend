using API.Features.User.Password.Reset.Models.Services;
using API.Features.User.Password.Reset.Services;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Email;
using API.Shared.Interfaces.Security;
using API.Shared.Models.Infrastructure.Hasher;
using NSubstitute;
using NUnit.Framework;

namespace API.UnitTests.Features.User.Password.Reset.Services;

public class ResetPasswordServiceTests : TestBase
{
    #region Fields

    private ResetPasswordService? _resetPasswordService;
    private IUserRepository? _userRepository;
    private IVerificationRepository? _verificationRepository;
    private IRandomGenerator? _randomGenerator;
    private IHasher? _hasher;
    private IEmailSender? _emailSender;

    #endregion

    #region Setup

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _userRepository = Substitute.For<IUserRepository>();
        _verificationRepository = Substitute.For<IVerificationRepository>();
        _randomGenerator = Substitute.For<IRandomGenerator>();
        _hasher = Substitute.For<IHasher>();
        _emailSender = Substitute.For<IEmailSender>();

        _resetPasswordService = new ResetPasswordService(
            _userRepository,
            _verificationRepository,
            _randomGenerator,
            _hasher,
            _emailSender);
    }

    #endregion

    #region Helper Methods

    private API.Infrastructure.Database.Entities.User.User CreateMockUser(UserState state = UserState.Active)
    {
        return new API.Infrastructure.Database.Entities.User.User
        {
            Id = 1,
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedPassword",
            PasswordSalt = "salt",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Gender = UserGender.Male,
            State = state,
            EmailAddresses = new List<EmailAddress>()
        };
    }

    private EmailAddress CreateMockEmailAddress(string email, API.Infrastructure.Database.Entities.User.User user, EmailState state = EmailState.Active, EmailType type = EmailType.Primary)
    {
        return new EmailAddress
        {
            Id = 1,
            Value = email,
            State = state,
            Type = type,
            User = user,
            UserId = user.Id
        };
    }

    private PasswordResetRequest CreateMockPasswordResetRequest(HashResult codeHash, API.Infrastructure.Database.Entities.User.User user, EmailAddress emailAddress, bool isUsed = false, bool isExpired = false)
    {
        return new PasswordResetRequest
        {
            Id = 1,
            CodeHash = codeHash.Hash,
            ExpiresAt = isExpired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(1),
            IsUsed = isUsed,
            User = user,
            UserId = user.Id,
            EmailAddress = emailAddress,
            EmailId = emailAddress.Id
        };
    }

    private HashResult CreateMockHashedValue(string hash = "hashedCode", string salt = "")
    {
        return new HashResult { Hash = hash, Salt = salt, IsSuccess = true, Status = "OK" };
    }

    #endregion

    #region RequestPasswordResetViaEmailAsync Tests

    [Test]
    public async Task RequestPasswordResetViaEmailAsync_ValidEmail_ReturnsSuccessResult()
    {
        // Arrange
        var email = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress(email, mockUser);
        mockUser.EmailAddresses.Add(mockEmailAddress);
        var code = "generatedCode";
        var codeHash = CreateMockHashedValue();

        _userRepository!.GetEmailAdressByEmailStringAsync(email, true).Returns(mockEmailAddress);
        _randomGenerator!.GenerateAlphanumericCode(64).Returns(code);
        _hasher!.Hash(code, "").Returns(codeHash);
        _emailSender!.SendResetPasswordEmailAsync(email, code, "en").Returns(true);

        // Act
        var result = await _resetPasswordService!.RequestPasswordResetViaEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("SUCCESS", result.Status);
        Assert.AreEqual("Password reset request sent", result.Message);
        Assert.AreEqual(200, result.HttpStatusCode);

        await _verificationRepository!.Received(1).AddPasswordResetRequestAsync(Arg.Any<PasswordResetRequest>());
    }

    [Test]
    public async Task RequestPasswordResetViaEmailAsync_EmailNotFound_ReturnsEmailNotFoundResult()
    {
        // Arrange
        var email = "nonexistent@example.com";
        
        _userRepository!.GetEmailAdressByEmailStringAsync(email, true).Returns((EmailAddress)null!);

        // Act
        var result = await _resetPasswordService!.RequestPasswordResetViaEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Email not found", result.Message);
        Assert.AreEqual(404, result.HttpStatusCode);
    }

    [Test]
    public async Task RequestPasswordResetViaEmailAsync_UserNotFound_ReturnsUserNotFoundResult()
    {
        // Arrange
        var email = "test@example.com";
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var mockEmailAddress = new EmailAddress
        {
            Id = 1,
            Value = email,
            State = EmailState.Active,
            Type = EmailType.Primary,
            User = null,
            UserId = 1
        };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        _userRepository!.GetEmailAdressByEmailStringAsync(email, true).Returns(mockEmailAddress);

        // Act
        var result = await _resetPasswordService!.RequestPasswordResetViaEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("User not found", result.Message);
        Assert.AreEqual(404, result.HttpStatusCode);
    }

    [Test]
    public async Task RequestPasswordResetViaEmailAsync_EmailNotActive_ReturnsEmailNotActiveResult()
    {
        // Arrange
        var email = "inactive@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress(email, mockUser, EmailState.Disabled);

        _userRepository!.GetEmailAdressByEmailStringAsync(email, true).Returns(mockEmailAddress);

        // Act
        var result = await _resetPasswordService!.RequestPasswordResetViaEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("This email is not active and cannot be used to reset your password", result.Message);
        Assert.AreEqual(400, result.HttpStatusCode);
    }

    [Test]
    public async Task RequestPasswordResetViaEmailAsync_EmailSendingFails_ReturnsEmailSendingFailedResult()
    {
        // Arrange
        var email = "test@example.com";
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress(email, mockUser);
        var code = "generatedCode";
        var codeHash = CreateMockHashedValue();

        _userRepository!.GetEmailAdressByEmailStringAsync(email, true).Returns(mockEmailAddress);
        _randomGenerator!.GenerateAlphanumericCode(64).Returns(code);
        _hasher!.Hash(code, "").Returns(codeHash);
        _emailSender!.SendResetPasswordEmailAsync(email, code, "en").Returns(false);

        // Act
        var result = await _resetPasswordService!.RequestPasswordResetViaEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Email cannot be sent", result.Message);
        Assert.AreEqual(500, result.HttpStatusCode);
    }

    [Test]
    public async Task RequestPasswordResetViaEmailAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        var email = "test@example.com";
        
        _userRepository!.When(x => x.GetEmailAdressByEmailStringAsync(email, true))
            .Do(x => { throw new Exception("Test exception"); });

        // Act
        var result = await _resetPasswordService!.RequestPasswordResetViaEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Test exception", result.Message);
        Assert.AreEqual(500, result.HttpStatusCode);
    }

    #endregion

    #region CheckRequestStatusAsync Tests

    [Test]
    public async Task CheckRequestStatusAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var code = "testCode";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress);

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash).Returns(mockRequest);

        // Act
        var result = await _resetPasswordService!.CheckRequestStatusAsync(code, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("SUCCESS", result.Status);
        Assert.AreEqual("Request found", result.Message);
        Assert.AreEqual(200, result.HttpStatusCode);
        Assert.IsFalse(result.IsExpired);
        Assert.IsFalse(result.IsUsed);
    }

    [Test]
    public async Task CheckRequestStatusAsync_RequestNotFound_ReturnsRequestNotFoundResult()
    {
        // Arrange
        var code = "nonExistentCode";
        var codeHash = CreateMockHashedValue();

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash).Returns((PasswordResetRequest)null!);

        // Act
        var result = await _resetPasswordService!.CheckRequestStatusAsync(code, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Request not found", result.Message);
        Assert.AreEqual(404, result.HttpStatusCode);
    }

    [Test]
    public async Task CheckRequestStatusAsync_RequestExpired_ReturnsRequestExpiredResult()
    {
        // Arrange
        var code = "expiredCode";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress, isExpired: true);

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash).Returns(mockRequest);

        // Act
        var result = await _resetPasswordService!.CheckRequestStatusAsync(code, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("SUCCESS", result.Status);
        Assert.AreEqual("Request found", result.Message);
        Assert.AreEqual(200, result.HttpStatusCode);
        Assert.IsTrue(result.IsExpired);
        Assert.IsFalse(result.IsUsed);
    }

    [Test]
    public async Task CheckRequestStatusAsync_RequestUsed_ReturnsRequestUsedResult()
    {
        // Arrange
        var code = "usedCode";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress, isUsed: true);

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash).Returns(mockRequest);

        // Act
        var result = await _resetPasswordService!.CheckRequestStatusAsync(code, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("SUCCESS", result.Status);
        Assert.AreEqual("Request found", result.Message);
        Assert.AreEqual(200, result.HttpStatusCode);
        Assert.IsFalse(result.IsExpired);
        Assert.IsTrue(result.IsUsed);
    }

    [Test]
    public async Task CheckRequestStatusAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        var code = "testCode";
        var codeHash = CreateMockHashedValue();

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.When(x => x.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash))
            .Do(x => { throw new Exception("Test exception"); });

        // Act
        var result = await _resetPasswordService!.CheckRequestStatusAsync(code, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Test exception", result.Message);
        Assert.AreEqual(500, result.HttpStatusCode);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Test]
    public async Task ResetPasswordAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var code = "testCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress);
        var passwordHash = CreateMockHashedValue("hashedPassword", "salt");

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(
            Arg.Any<string>(), 
            includeUser: Arg.Any<bool>(), 
            includeEmailAddress: Arg.Any<bool>())
            .Returns(mockRequest);
        _userRepository!.GetEmailAdressByIdAsync(mockEmailAddress.Id, Arg.Any<bool>()).Returns(mockEmailAddress);
        _hasher!.Hash(newPassword).Returns(passwordHash);

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("SUCCESS", result.Status);
        Assert.AreEqual("Password reset successfully", result.Message);
        Assert.AreEqual(200, result.HttpStatusCode);

        await _userRepository!.Received(1).UpdateUserPasswordAsync(mockUser.Id, passwordHash.Hash, passwordHash.Salt);
        await _verificationRepository!.Received(1).MarkPasswordResetRequestAsUsedAsync(mockRequest.Id);
    }

    [Test]
    public async Task ResetPasswordAsync_RequestNotFound_ReturnsRequestNotFoundResult()
    {
        // Arrange
        var code = "nonExistentCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash, true).Returns((PasswordResetRequest)null!);

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Request not found", result.Message);
        Assert.AreEqual(404, result.HttpStatusCode);
    }

    [Test]
    public async Task ResetPasswordAsync_RequestExpired_ReturnsRequestExpiredResult()
    {
        // Arrange
        var code = "expiredCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress, isExpired: true);

        _hasher!.Hash(code, "").Returns(codeHash);
        
        // Use a more explicit approach for setting up the mock - specify both boolean parameters
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(
            Arg.Any<string>(), 
            includeUser: Arg.Any<bool>(), 
            includeEmailAddress: Arg.Any<bool>())
            .Returns(mockRequest);

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Request expired", result.Message);
        Assert.AreEqual(400, result.HttpStatusCode);
    }

    [Test]
    public async Task ResetPasswordAsync_RequestUsed_ReturnsRequestUsedResult()
    {
        // Arrange
        var code = "usedCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress, isUsed: true);

        _hasher!.Hash(code, "").Returns(codeHash);
        
        // Use a more explicit approach for setting up the mock - specify both boolean parameters
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(
            Arg.Any<string>(), 
            includeUser: Arg.Any<bool>(), 
            includeEmailAddress: Arg.Any<bool>())
            .Returns(mockRequest);

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Request already used", result.Message);
        Assert.AreEqual(400, result.HttpStatusCode);
    }

    [Test]
    public async Task ResetPasswordAsync_EmailNotFound_ReturnsEmailNotFoundResult()
    {
        // Arrange
        var code = "testCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress);

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(
            Arg.Any<string>(), 
            includeUser: Arg.Any<bool>(), 
            includeEmailAddress: Arg.Any<bool>())
            .Returns(mockRequest);
        _userRepository!.GetEmailAdressByIdAsync(mockEmailAddress.Id, Arg.Any<bool>()).Returns((EmailAddress)null!);

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("Email not found", result.Message);
        Assert.AreEqual(404, result.HttpStatusCode);
    }

    [Test]
    public async Task ResetPasswordAsync_UserNotFound_ReturnsUserNotFoundResult()
    {
        // Arrange
        var code = "testCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();
        var mockUser = CreateMockUser();
        var mockEmailAddress = CreateMockEmailAddress("test@example.com", mockUser);
        var mockRequest = CreateMockPasswordResetRequest(codeHash, mockUser, mockEmailAddress);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var emailWithoutUser = new EmailAddress
        {
            Id = mockEmailAddress.Id,
            Value = mockEmailAddress.Value,
            State = mockEmailAddress.State,
            Type = mockEmailAddress.Type,
            User = null,
            UserId = mockUser.Id
        };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        _hasher!.Hash(code, "").Returns(codeHash);
        _verificationRepository!.GetPasswordResetRequestByCodeHashAsync(
            Arg.Any<string>(), 
            includeUser: Arg.Any<bool>(), 
            includeEmailAddress: Arg.Any<bool>())
            .Returns(mockRequest);
        _userRepository!.GetEmailAdressByIdAsync(mockEmailAddress.Id, Arg.Any<bool>()).Returns(emailWithoutUser);

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual("User not found", result.Message);
        Assert.AreEqual(404, result.HttpStatusCode);
    }

    [Test]
    public async Task ResetPasswordAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        var code = "testCode";
        var newPassword = "NewPassword123!";
        var codeHash = CreateMockHashedValue();
        var exceptionMessage = "Test exception";

        _hasher!.Hash(code, "").Returns(codeHash);
        
        // Set up the mock to throw an exception when called
        _verificationRepository!.When(x => 
            x.GetPasswordResetRequestByCodeHashAsync(
                Arg.Any<string>(), 
                Arg.Any<bool>(), 
                Arg.Any<bool>()))
            .Do(x => { throw new Exception(exceptionMessage); });

        // Act
        var result = await _resetPasswordService!.ResetPasswordAsync(code, newPassword, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("ERROR", result.Status);
        Assert.AreEqual(exceptionMessage, result.Message);
        Assert.AreEqual(500, result.HttpStatusCode);
    }

    #endregion
}
