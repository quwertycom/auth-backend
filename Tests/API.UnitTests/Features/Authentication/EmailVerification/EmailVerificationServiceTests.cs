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

    [Test]
    public async Task RequestEmailVerificationAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        string emailAddress = "test@example.com";
        var mockEmailAddressEntity = new EmailAddress {
            Id = Snowflake.Generate(),
            UserId = Snowflake.Generate(),
            Value = emailAddress,
            State = EmailState.PendingVerification,
            Type = EmailType.Primary,
            User = new User() {
                Id = Snowflake.Generate(),
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                BirthDate = DateTime.Now.AddYears(-20),
                Gender = UserGender.Male,
                State = UserState.PendingVerification
            }
        };
        var mockUserEntity = mockEmailAddressEntity.User;

        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(mockUserEntity));

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
            req.User == mockUserEntity &&
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
        var mockEmailAddressEntity = new EmailAddress {
            Id = Snowflake.Generate(),
            UserId = Snowflake.Generate(),
            Value = emailAddress,
            State = EmailState.PendingVerification,
            Type = EmailType.Primary,
            User = new User() {
                Id = Snowflake.Generate(),
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                BirthDate = DateTime.Now.AddYears(-20),
                Gender = UserGender.Male,
                State = UserState.PendingVerification
            }
        };
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
        var mockEmailAddressEntity = new EmailAddress {
            Id = Snowflake.Generate(),
            UserId = Snowflake.Generate(),
            Value = emailAddress,
            State = EmailState.Active,
            Type = EmailType.Primary,
            User = new User() {
                Id = Snowflake.Generate(),
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                BirthDate = DateTime.Now.AddYears(-20),
                Gender = UserGender.Male,
                State = UserState.PendingVerification
            }
        };
        var mockUserEntity = mockEmailAddressEntity.User;

        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(mockUserEntity));

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
        var mockEmailAddressEntity = new EmailAddress {
            Id = Snowflake.Generate(),
            UserId = Snowflake.Generate(),
            Value = emailAddress,
            State = EmailState.PendingVerification,
            Type = EmailType.Primary,
            User = new User() {
                Id = Snowflake.Generate(),
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                BirthDate = DateTime.Now.AddYears(-20),
                Gender = UserGender.Male,
                State = UserState.PendingVerification
            }
        };
        var mockUserEntity = mockEmailAddressEntity.User;

        _mockUserRepository!.GetEmailAdressByEmailStringAsync(emailAddress).Returns(Task.FromResult<EmailAddress?>(mockEmailAddressEntity));
        _mockUserRepository!.GetUserByIdAsync(mockEmailAddressEntity.UserId).Returns(Task.FromResult<User?>(mockUserEntity));
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
}
