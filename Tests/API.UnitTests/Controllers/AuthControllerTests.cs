using API.Web.Controllers;
using API.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using API.Core.Contracts.Requests.Auth;
using API.Core.Contracts.Responses.Auth;
using API.Core.Contracts.Responses.Common;
using API.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace API.UnitTests.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly AuthController _controller;
    private readonly IAuthService _mockAuthService;

    public AuthControllerTests()
    {
        _mockAuthService = GetMock<IAuthService>();
        _controller = new AuthController(_mockAuthService);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidRequest_ReturnsOkResult()
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

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((true, "OTP_SENT", "OTP message", 123));

        // Act
        var result = await _controller.RegisterUserAsync(request) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var response = result.Value as RegisterResponse;
        Assert.NotNull(response);
        Assert.Equal("SUCCESS", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "USERNAME_INVALID", "Username invalid", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("USERNAME_INVALID", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_PasswordTooShort_ReturnsBadRequest()
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

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "PASSWORD_TOO_SHORT", "Password too short", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("PASSWORD_TOO_SHORT", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_InternalServerError_ReturnsStatusCode500()
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

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "INTERNAL_SERVER_ERROR", "Internal server error", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("INTERNAL_SERVER_ERROR", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailTaken_ReturnsBadRequest()
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

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "EMAIL_TAKEN", "Email already exists", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("EMAIL_TAKEN", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidEmailFormat_ReturnsBadRequest()
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

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "INVALID_EMAIL", "Invalid email format.", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("INVALID_EMAIL", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_ShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Short",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "PASSWORD_TOO_SHORT", "Password must be at least 8 characters.", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("PASSWORD_TOO_SHORT", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_LongUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = new string('A', 51), // Username longer than 50 characters
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "USERNAME_INVALID", "Username is too long.", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("USERNAME_INVALID", response.Status);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidCharactersInUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "test!user", // Username with invalid characters
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        _mockAuthService.RegisterUserAsync(Arg.Any<RegisterRequest>())
            .Returns((false, "USERNAME_INVALID", "Username contains invalid characters.", null));

        // Act
        var result = await _controller.RegisterUserAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("USERNAME_INVALID", response.Status);
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            Email = "test@example.com",
            OTP = "12345678",
            VerificationSessionID = 123
        };

        _mockAuthService.VerifyEmailAsync(Arg.Any<VerifyEmailRequest>())
            .Returns((true, "SUCCESS", "Email verified successfully.", 123));

        // Act
        var result = await _controller.VerifyEmailAsync(request) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var response = result.Value as VerifyEmailResponse;
        Assert.NotNull(response);
        Assert.Equal("SUCCESS", response.Status);
        Assert.Equal("test@example.com", response.Email);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidOTP_ReturnsBadRequest()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 1,
            Email = "test@example.com",
            OTP = "InvalidOTP"
        };

        _mockAuthService.VerifyEmailAsync(Arg.Any<VerifyEmailRequest>())
            .Returns((false, "INVALID_OTP", "Invalid OTP.", null));

        // Act
        var result = await _controller.VerifyEmailAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("INVALID_OTP", response.Status);
    }

    [Fact]
    public async Task VerifyEmailAsync_SessionNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            VerificationSessionID = 999,
            Email = "test@example.com",
            OTP = "12345678"
        };

        _mockAuthService.VerifyEmailAsync(Arg.Any<VerifyEmailRequest>())
            .Returns((false, "VERIFICATION_SESSION_NOT_FOUND", "Verification session not found.", null));

        // Act
        var result = await _controller.VerifyEmailAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("VERIFICATION_SESSION_NOT_FOUND", response.Status);
    }

    [Fact]
    public async Task VerifyEmailAsync_InternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            Email = "test@example.com",
            OTP = "12345678",
            VerificationSessionID = 123
        };

        _mockAuthService.VerifyEmailAsync(Arg.Any<VerifyEmailRequest>())
            .Returns((false, "INTERNAL_SERVER_ERROR", "Something went wrong", null));

        // Act
        var result = await _controller.VerifyEmailAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("INTERNAL_SERVER_ERROR", response.Status);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123"
        };

        _mockAuthService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns((true, "SUCCESS", "Login successful", "accessToken", "refreshToken"));

        // Act
        var result = await _controller.LoginAsync(request) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var response = result.Value as LoginResponse;
        Assert.NotNull(response);
        Assert.Equal("SUCCESS", response.Status);
        Assert.Equal("accessToken", response.AccessToken);
        Assert.Equal("refreshToken", response.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongPassword"
        };

        _mockAuthService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns((false, "INVALID_PASSWORD", "Invalid credentials.", null, null));

        // Act
        var result = await _controller.LoginAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("INVALID_PASSWORD", response.Status);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "anyPassword"
        };

        _mockAuthService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns((false, "NOT_FOUND", "User not found.", null, null));

        // Act
        var result = await _controller.LoginAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("NOT_FOUND", response.Status);
    }

    [Fact]
    public async Task LoginAsync_InternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123"
        };

        _mockAuthService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns((false, "INTERNAL_SERVER_ERROR", "Something went wrong", null, null));

        // Act
        var result = await _controller.LoginAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("INTERNAL_SERVER_ERROR", response.Status);
    }

    [Fact]
    public async Task LoginAsync_AccountLocked_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "lockeduser",
            Password = "Password123"
        };

        _mockAuthService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns((false, "ACCOUNT_LOCKED", "Account is locked.", null, null));

        // Act
        var result = await _controller.LoginAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("ACCOUNT_LOCKED", response.Status);
    }

    [Fact]
    public async Task LoginAsync_UserNotActive_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "inactiveuser",
            Password = "Password123"
        };

        _mockAuthService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns((false, "USER_INACTIVE", "User is not active.", null, null));

        // Act
        var result = await _controller.LoginAsync(request) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        var response = result.Value as ErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("USER_INACTIVE", response.Status);
    }
}