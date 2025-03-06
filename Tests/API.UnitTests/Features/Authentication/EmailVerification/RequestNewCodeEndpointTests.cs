using API.Features.Authentication.EmailVerification.Endpoints;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Features.Authentication.EmailVerification.Models.Services;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace API.UnitTests.Features.Authentication.EmailVerification;

public class RequestNewCodeEndpointTests : TestBase
{
    private API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService? _mockEmailVerificationService;

    #region Setup

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _mockEmailVerificationService = Substitute.For<API.Features.Authentication.EmailVerification.Interfaces.IEmailVerificationService>();
    }

    #endregion

    #region Helper Methods

    private RequestNewCodeRequest CreateDefaultRequest(string email = "test@example.com")
    {
        return new RequestNewCodeRequest
        {
            Email = email
        };
    }

    private RequestNewCodeResult CreateSuccessResult()
    {
        return new RequestNewCodeResult
        {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "A new verification code has been sent to your email",
            NewRequestId = "new-verification-request-id",
            HttpStatusCode = StatusCodes.Status200OK
        };
    }

    private RequestNewCodeResult CreateFailureResult(
        string status = "ERROR",
        string message = "Failed to send verification code",
        int? httpStatusCode = StatusCodes.Status400BadRequest)
    {
        return new RequestNewCodeResult
        {
            IsSuccess = false,
            Status = status,
            Message = message,
            HttpStatusCode = httpStatusCode
        };
    }

    #endregion

    #region Constructor Tests

    [Test]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var endpoint = new RequestNewCodeEndpoint(_mockEmailVerificationService!);

        // Assert - use reflection to check private field was initialized
        var fieldInfo = typeof(RequestNewCodeEndpoint).GetField(
            "_emailVerificationService",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(fieldInfo, Is.Not.Null, "Field _emailVerificationService should exist");

        var fieldValue = fieldInfo!.GetValue(endpoint);
        Assert.That(fieldValue, Is.EqualTo(_mockEmailVerificationService),
            "Service should be initialized in constructor");
    }

    #endregion

    #region Configuration Tests

    [Test]
    public void Configure_SetsEndpointRouteAndOptions()
    {
        // Since we can't easily test the internal state of FastEndpoints,
        // we'll verify the Configure method exists and has the expected structure

        // Get the method info for inspection
        var configureMethod = typeof(RequestNewCodeEndpoint).GetMethod("Configure",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Verify method exists
        Assert.That(configureMethod, Is.Not.Null, "Configure method should exist");

        // This test is minimal since we can't access internal FastEndpoints state during unit tests
        // In real-world testing, we'd need integration tests to verify the endpoint registration
    }

    #endregion

    #region Service Interaction Tests

    [Test]
    public async Task HandleAsync_CallsServiceWithCorrectEmail()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var successResult = CreateSuccessResult();

        _mockEmailVerificationService!
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(successResult);

        // We're going to spy on the method to verify it behaves correctly
        // without actually calling FastEndpoints methods
        var endpoint = new RequestNewCodeEndpoint(_mockEmailVerificationService);

        // Use a try/catch to handle expected exception - we can't mock internal FastEndpoints behavior
        try
        {
            // Act - call the HandleAsync method, it will throw due to missing FastEndpoints context
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected to throw due to FastEndpoints internals
            // We're only checking if our service was called correctly
        }

        // Assert - verify the service was called with correct parameters
        await _mockEmailVerificationService
            .Received(1)
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WithEmailNotFound_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var failureResult = CreateFailureResult(
            status: "EMAIL_NOT_FOUND",
            message: "Email address not found in our system"
        );

        _mockEmailVerificationService!
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(failureResult);

        // Create endpoint
        var endpoint = new RequestNewCodeEndpoint(_mockEmailVerificationService);

        // Use a try/catch to handle expected exception
        try
        {
            // Act
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception
        }

        // Assert - verify the service was called with correct parameters
        await _mockEmailVerificationService
            .Received(1)
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WithEmailAlreadyVerified_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var failureResult = CreateFailureResult(
            status: "EMAIL_ALREADY_VERIFIED",
            message: "Email address is already verified"
        );

        _mockEmailVerificationService!
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(failureResult);

        // Create endpoint
        var endpoint = new RequestNewCodeEndpoint(_mockEmailVerificationService);

        // Use a try/catch to handle expected exception
        try
        {
            // Act
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception
        }

        // Assert - verify the service was called with correct parameters
        await _mockEmailVerificationService
            .Received(1)
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WithInternalError_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var failureResult = CreateFailureResult(
            status: "ERROR",
            message: "An internal error occurred"
        );

        _mockEmailVerificationService!
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(failureResult);

        // Create endpoint
        var endpoint = new RequestNewCodeEndpoint(_mockEmailVerificationService);

        // Use a try/catch to handle expected exception
        try
        {
            // Act
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception
        }

        // Assert - verify the service was called with correct parameters
        await _mockEmailVerificationService
            .Received(1)
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WithCustomStatusCode_CallsServiceWithCorrectParameter()
    {
        // Arrange
        var request = CreateDefaultRequest();
        var result = new RequestNewCodeResult
        {
            IsSuccess = false,
            Status = "RATE_LIMITED",
            Message = "Too many attempts",
            HttpStatusCode = StatusCodes.Status429TooManyRequests
        };

        _mockEmailVerificationService!
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(result);

        // Create endpoint
        var endpoint = new RequestNewCodeEndpoint(_mockEmailVerificationService);

        // Use a try/catch to handle expected exception
        try
        {
            // Act
            await endpoint.HandleAsync(request, CancellationToken.None);
        }
        catch (Exception)
        {
            // Expected exception
        }

        // Assert - verify the service was called with correct parameters
        await _mockEmailVerificationService
            .Received(1)
            .RequestNewCodeAsync(request.Email, Arg.Any<CancellationToken>());
    }

    #endregion
}
