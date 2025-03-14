using API.Features.Authentication.Register.Services;
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.EmailVerification.Models.Services;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;
using API.Shared.Models.Infrastructure.Hasher;

namespace API.Tests.Unit.Features.Authentication.Register;

public class RegisterServiceTests : TestBase
{
    #region Helper Methods

    private RegisterRequest CreateDefaultRegisterRequest(
        string username = "testuser",
        string firstName = "Test",
        string lastName = "User",
        string email = "test@example.com",
        string password = "Password123!",
        string? phoneNumber = null,
        DateTime? birthDate = null,
        UserGender gender = UserGender.Male)
    {
        return new RegisterRequest
        {
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            PhoneNumber = phoneNumber,
            BirthDate = birthDate ?? DateTime.Now.AddYears(-20),
            Gender = gender
        };
    }

    private void SetupRepositoriesForSuccess()
    {
        MockUserRepository.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        MockUserRepository.EmailAdressExistsAsync(Arg.Any<string>()).Returns(false);
        MockUserRepository.PhoneNumberExistsAsync(Arg.Any<string>()).Returns(false);
    }

    #endregion

    #region Successful Registration Tests

    [Test]
    public async Task RegisterUserAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        SetupRepositoriesForSuccess();
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.RequestId.Should().Be("verification-session-id");

        // Verify interactions with repositories
        await MockUserRepository.Received(1).AddUserAsync(Arg.Is<API.Infrastructure.Database.Entities.User.User>(u =>
            u.Username == request.Username &&
            u.FirstName == request.FirstName &&
            u.LastName == request.LastName &&
            u.PasswordHash == "hashedpassword" &&
            u.PasswordSalt == "salt" &&
            u.BirthDate == request.BirthDate &&
            u.Gender == request.Gender &&
            u.State == UserState.PendingVerification
        ));

        await MockUserRepository.Received(1).AddEmailAsync(Arg.Is<EmailAddress>(e =>
            e.Value == request.Email &&
            e.State == EmailState.PendingVerification &&
            e.Type == EmailType.Primary
        ));

        // Since the request doesn't have a phone number, shouldn't call this
        await MockUserRepository.DidNotReceive().AddPhoneNumberAsync(Arg.Any<PhoneNumber>());
    }

    [Test]
    public async Task RegisterUserAsync_WithPhoneNumber_AddsPhoneNumber()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(phoneNumber: "+1234567890");
        SetupRepositoriesForSuccess();
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify phone number was added
        await MockUserRepository.Received(1).AddPhoneNumberAsync(Arg.Is<PhoneNumber>(p =>
            p.Value == request.PhoneNumber &&
            p.State == PhoneState.PendingVerification &&
            p.Type == PhoneType.Primary
        ));
    }

    #endregion

    #region Registration Failure Tests

    [Test]
    public async Task RegisterUserAsync_UsernameExists_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(username: "existinguser");
        MockUserRepository.UsernameExistsAsync("existinguser").Returns(true);
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("USERNAME_EXISTS");

        // Verify no user was added
        await MockUserRepository.DidNotReceive().AddUserAsync(Arg.Any<API.Infrastructure.Database.Entities.User.User>());
        await MockUserRepository.DidNotReceive().AddEmailAsync(Arg.Any<EmailAddress>());
    }

    [Test]
    public async Task RegisterUserAsync_EmailExists_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(email: "existing@example.com");
        MockUserRepository.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        MockUserRepository.EmailAdressExistsAsync("existing@example.com").Returns(true);
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_EXISTS");

        // Verify no user was added
        await MockUserRepository.DidNotReceive().AddUserAsync(Arg.Any<API.Infrastructure.Database.Entities.User.User>());
        await MockUserRepository.DidNotReceive().AddEmailAsync(Arg.Any<EmailAddress>());
    }

    [Test]
    public async Task RegisterUserAsync_PhoneNumberExists_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(phoneNumber: "+1234567890");
        MockUserRepository.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        MockUserRepository.EmailAdressExistsAsync(Arg.Any<string>()).Returns(false);
        MockUserRepository.PhoneNumberExistsAsync("+1234567890").Returns(true);
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("PHONE_NUMBER_EXISTS");

        // Verify no user was added
        await MockUserRepository.DidNotReceive().AddUserAsync(Arg.Any<API.Infrastructure.Database.Entities.User.User>());
        await MockUserRepository.DidNotReceive().AddEmailAsync(Arg.Any<EmailAddress>());
    }

    [Test]
    public async Task RegisterUserAsync_EmailVerificationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        SetupRepositoriesForSuccess();
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = false,
                Status = "EMAIL_VERIFICATION_FAILED",
                Message = "Could not send verification email"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("EMAIL_VERIFICATION_FAILED");
        result.Message.Should().Be("Could not send verification email");
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task RegisterUserAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        MockUserRepository.UsernameExistsAsync(Arg.Any<string>())
            .Returns(Task.FromException<bool>(new Exception("Database connection failed")));
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Database connection failed");
    }

    [Test]
    public async Task RegisterUserAsync_AddUserAsyncThrowsException_ReturnsErrorResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        SetupRepositoriesForSuccess();
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        MockUserRepository.AddUserAsync(Arg.Any<API.Infrastructure.Database.Entities.User.User>())
            .Returns(Task.FromException(new Exception("Failed to add user to database")));

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Failed to add user to database");
    }

    [Test]
    public async Task RegisterUserAsync_AddEmailAsyncThrowsException_ReturnsErrorResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        SetupRepositoriesForSuccess();
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        MockUserRepository.AddEmailAsync(Arg.Any<EmailAddress>())
            .Returns(Task.FromException(new Exception("Failed to add email to database")));

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Failed to add email to database");
    }

    [Test]
    public async Task RegisterUserAsync_AddPhoneNumberAsyncThrowsException_ReturnsErrorResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(phoneNumber: "+1234567890");
        SetupRepositoriesForSuccess();
        var mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
        var registerService = new RegisterService(
            MockUserRepository,
            MockHasher,
            mockEmailVerificationService
        );

        // Setup default behavior for the Hasher
        MockHasher.Hash(Arg.Any<string>()).Returns(new HashResult
        {
            IsSuccess = true,
            Status = "OK",
            Hash = "hashedpassword",
            Salt = "salt"
        });

        // Setup default behavior for email verification service
        mockEmailVerificationService
            .RequestEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RequestEmailVerificationResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                RequestId = "verification-session-id"
            });

        MockUserRepository.AddPhoneNumberAsync(Arg.Any<PhoneNumber>())
            .Returns(Task.FromException(new Exception("Failed to add phone number to database")));

        // Act
        var result = await registerService!.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Contain("Failed to add phone number to database");
    }

    #endregion
}
