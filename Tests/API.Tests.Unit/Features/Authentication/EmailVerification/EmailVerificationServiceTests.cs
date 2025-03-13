using API.Features.Authentication.EmailVerification.Services;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Email;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;
using API.Shared.Utilities;
using System.Reflection.Metadata;

namespace API.Tests.Unit.Features.Authentication.EmailVerification;

public class EmailVerificationServiceTests : TestBase
{
    #region RequestEmailVerificationAsync Tests

    [Test]
    public async Task RequestEmailVerificationAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: emailAddress, state: EmailState.PendingVerification, user: mockUser);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<API.Infrastructure.Database.Entities.User.User?>(mockUser));
        mockRandomGenerator.GenerateNumberCode(8).Returns("12345678");
        mockEmailSender.SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);


        // Act
        var result = await emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.RequestId.Should().NotBeNullOrEmpty();

        await mockVerificationRepository!.Received(1).AddEmailVerificationRequestAsync(Arg.Is<EmailVerificationRequest>(req =>
            req.Code == "12345678" &&
            req.EmailAddress == mockEmailAddressEntity &&
            req.User == mockUser &&
            req.ExpiresAt > DateTime.UtcNow &&
            req.CreatedAt <= DateTime.UtcNow
        ));
        await mockEmailSender!.Received(1).SendOtpEmailAsync(emailAddress, "12345678", "Test", "en");
    }

    [Test]
    public async Task RequestEmailVerificationAsync_EmailNotFound_ReturnsEmailNotFoundResult()
    {
        // Arrange
        string emailAddress = "nonexistent@example.com";
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );
        mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(null));

        // Act
        var result = await emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_NOT_FOUND");
        result.RequestId.Should().BeNull();

        await mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestEmailVerificationAsync_UserNotFound_ReturnsUserNotFoundResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: emailAddress, state: EmailState.PendingVerification, user: mockUser);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<API.Infrastructure.Database.Entities.User.User?>(null));

        // Act
        var result = await emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("USER_NOT_FOUND");
        result.RequestId.Should().BeNull();

        await mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestEmailVerificationAsync_EmailNotPendingVerification_ReturnsEmailNotVerifiedResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: emailAddress, state: EmailState.Active, user: mockUser);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<API.Infrastructure.Database.Entities.User.User?>(mockUser));

        // Act
        var result = await emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_NOT_VERIFIED");
        result.RequestId.Should().BeNull();

        await mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestEmailVerificationAsync_EmailSendingFails_ReturnsEmailSendingFailedResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: emailAddress, state: EmailState.PendingVerification, user: mockUser);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<API.Infrastructure.Database.Entities.User.User?>(mockUser));
        mockRandomGenerator.GenerateNumberCode(8).Returns("12345678");
        mockEmailSender!.SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        var result = await emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_SENDING_FAILED");
        result.RequestId.Should().BeNull();

        await mockVerificationRepository!.Received(1).AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await mockEmailSender!.Received(1).SendOtpEmailAsync(emailAddress, "12345678", "Test", "en");
    }

    [Test]
    public async Task RequestEmailVerificationAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress)
            .Returns(Task.FromException<EmailAddress?>(new Exception("Database error")));

        // Act
        var result = await emailVerificationService!.RequestEmailVerificationAsync(emailAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.RequestId.Should().BeNull();

        await mockVerificationRepository!.DidNotReceive().AddEmailVerificationRequestAsync(Arg.Any<EmailVerificationRequest>());
        await mockEmailSender!.DidNotReceive().SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    #endregion

    #region GetRequestStatusAsync Tests

    [Test]
    public async Task GetRequestStatusAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string requestId = "123456789";
        string email = "test@example.com";
        var mockUser = _generate.NewUser();
        var emailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.PendingVerification, user: mockUser);
        var verificationRequest = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: "12345678", user: mockUser, emailAddress: emailAddressEntity);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));

        mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));

        // Act
        var result = await emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);

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
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(null));

        // Act
        var result = await emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);

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

        var mockUser = _generate.NewUser();
        var emailAddressEntity = _generate.NewEmailAddress(value: differentEmail, state: EmailState.PendingVerification, user: mockUser);
        var verificationRequest = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: "12345678", user: mockUser, emailAddress: emailAddressEntity);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));

        mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));

        // Act
        var result = await emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);

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

        var mockUser = _generate.NewUser();
        var emailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.PendingVerification, user: mockUser);
        var verificationRequest = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: "12345678", user: mockUser, emailAddress: emailAddressEntity, isUsed: true);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));

        mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));

        // Act
        var result = await emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);

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

        var mockUser = _generate.NewUser();
        var emailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.PendingVerification, user: mockUser);
        var verificationRequest = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: "12345678", user: mockUser, emailAddress: emailAddressEntity, expiresAt: DateTime.UtcNow.AddDays(-1));

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(verificationRequest));

        mockUserRepository!.GetEmailAdressByIdAsync(emailAddressEntity.Id)
            .Returns(Task.FromResult<EmailAddress?>(emailAddressEntity));

        // Act
        var result = await emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);

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
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true)
            .Returns(Task.FromException<EmailVerificationRequest?>(new Exception("Database error")));

        // Act
        var result = await emailVerificationService!.GetRequestStatusAsync(requestId, email, CancellationToken.None);

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

        var user = _generate.NewUser();
        user.Id = userId;

        var emailAddress = _generate.NewEmailAddress(value: "test@example.com", state: EmailState.PendingVerification, user: user);
        emailAddress.Id = emailId;

        var request = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: code, user: user, emailAddress: emailAddress);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));

        // Act
        var result = await emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.RequestId.Should().Be(requestId);

        await mockVerificationRepository!.Received(1).MarkEmailVerificationRequestAsUsedAsync(long.Parse(requestId));
        await mockUserRepository!.Received(1).UpdateUserStateAsync(userId, UserState.Active);
        await mockUserRepository!.Received(1).UpdateEmailStateAsync(emailId, EmailState.Active);
    }

    [Test]
    public async Task VerifyEmailAsync_RequestNotFound_ReturnsRequestNotFoundResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(null));

        // Act
        var result = await emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_NOT_FOUND");
        result.HttpStatusCode.Should().Be(404);

        await mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }

    [Test]
    public async Task VerifyEmailAsync_RequestUsed_ReturnsRequestUsedResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";

        var userId = Snowflake.Generate();
        var emailId = Snowflake.Generate();

        var user = _generate.NewUser();
        user.Id = userId;

        var emailAddress = _generate.NewEmailAddress(value: "test@example.com", state: EmailState.PendingVerification, user: user);
        emailAddress.Id = emailId;

        var request = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: code, user: user, emailAddress: emailAddress, isUsed: true);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));

        // Act
        var result = await emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_USED");
        result.HttpStatusCode.Should().Be(400);

        await mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }

    [Test]
    public async Task VerifyEmailAsync_RequestExpired_ReturnsRequestExpiredResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";

        var userId = Snowflake.Generate();
        var emailId = Snowflake.Generate();

        var user = _generate.NewUser();
        user.Id = userId;

        var emailAddress = _generate.NewEmailAddress(value: "test@example.com", state: EmailState.PendingVerification, user: user);
        emailAddress.Id = emailId;

        var request = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: code, user: user, emailAddress: emailAddress, expiresAt: DateTime.UtcNow.AddDays(-1));
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));

        // Act
        var result = await emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("REQUEST_EXPIRED");
        result.HttpStatusCode.Should().Be(410);

        await mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
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

        var user = _generate.NewUser();
        user.Id = userId;

        var emailAddress = _generate.NewEmailAddress(value: "test@example.com", state: EmailState.PendingVerification, user: user);
        emailAddress.Id = emailId;

        var request = _generate.NewEmailVerificationRequest(id: long.Parse(requestId), code: code, user: user, emailAddress: emailAddress);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromResult<EmailVerificationRequest?>(request));

        // Act
        var result = await emailVerificationService!.VerifyEmailAsync(requestId, incorrectCode, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("CODE_MISMATCH");
        result.HttpStatusCode.Should().Be(400);

        await mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }

    [Test]
    public async Task VerifyEmailAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        string requestId = "123456789";
        string code = "12345678";
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockVerificationRepository!.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true)
            .Returns(Task.FromException<EmailVerificationRequest?>(new Exception("Database error")));

        // Act
        var result = await emailVerificationService!.VerifyEmailAsync(requestId, code, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.HttpStatusCode.Should().Be(500);

        await mockVerificationRepository!.DidNotReceive().MarkEmailVerificationRequestAsUsedAsync(Arg.Any<long>());
        await mockUserRepository!.DidNotReceive().UpdateUserStateAsync(Arg.Any<long>(), Arg.Any<UserState>());
        await mockUserRepository!.DidNotReceive().UpdateEmailStateAsync(Arg.Any<long>(), Arg.Any<EmailState>());
    }

    #endregion

    #region RequestNewCodeAsync Tests

    [Test]
    public async Task RequestNewCodeAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string email = "test@example.com";
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.PendingVerification, user: mockUser);

        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));

        mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId)
            .Returns(Task.FromResult<API.Infrastructure.Database.Entities.User.User?>(mockEmailAddressEntity.User));

        mockRandomGenerator.GenerateNumberCode(8).Returns("12345678");
        mockEmailSender.SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

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
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(null));

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

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
        var mockUser = _generate.NewUser(state: UserState.Active);
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.Active, user: mockUser);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

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
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.Blacklisted, user: mockUser);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

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
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.Disabled, user: mockUser);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

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
        var mockUser = _generate.NewUser();
        var mockEmailAddressEntity = _generate.NewEmailAddress(value: email, state: EmailState.Deleted, user: mockUser);
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

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
        var mockUserRepository = Substitute.For<IUserRepository>();
        var mockVerificationRepository = Substitute.For<IVerificationRepository>();
        var mockRandomGenerator = Substitute.For<IRandomGenerator>();
        var mockEmailSender = Substitute.For<IEmailSender>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepository,
            mockVerificationRepository,
            mockRandomGenerator,
            mockEmailSender
        );

        mockUserRepository!.GetEmailAdressByEmailStringAsync(email)
            .Returns(Task.FromException<EmailAddress?>(new Exception("Database error")));

        // Act
        var result = await emailVerificationService!.RequestNewCodeAsync(email, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database error");
        result.HttpStatusCode.Should().Be(500);
    }

    #endregion
}
