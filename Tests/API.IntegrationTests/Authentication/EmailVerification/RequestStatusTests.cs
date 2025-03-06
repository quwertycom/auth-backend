using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using NUnit.Framework;

namespace API.IntegrationTests.Authentication.EmailVerification;

[TestFixture]
public class RequestStatusTests : TestBase
{
    [Test]
    public async Task RequestStatus_Endpoint_Should_BeAccessible()
    {
        // Act: Send request to the endpoint
        var response = await _client.GetAsync("/api/authentication/email-verification/request-status");

        // Assert: The endpoint exists and returns a response (likely BadRequest due to missing parameters)
        Assert.IsFalse(response.StatusCode == HttpStatusCode.NotFound, 
            "RequestStatus endpoint should exist and not return 404 Not Found");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestStatus endpoint should return BadRequest when called without parameters");
    }

    [Test]
    public async Task RequestStatus_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange - Create a verification request
        var (requestId, email) = await CreateVerificationRequestAsync();

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={requestId}&Email={email}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "RequestStatus with valid request should return OK");

        var content = await response.Content.ReadFromJsonAsync<RequestStatusResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
    }

    [Test]
    public async Task RequestStatus_InvalidRequestId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequestId = "invalid-id"; // Non-numeric request ID
        var email = "test@example.com";

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={invalidRequestId}&Email={email}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestStatus with invalid request ID format should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task RequestStatus_NonExistingRequest_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingRequestId = "999999999999"; // Non-existent request ID
        var email = "test@example.com";

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={nonExistingRequestId}&Email={email}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
            "RequestStatus with non-existing request ID should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("REQUEST_NOT_FOUND"));
    }

    [Test]
    public async Task RequestStatus_EmailMismatch_ShouldReturnBadRequest()
    {
        // Arrange - Create a verification request
        var (requestId, _) = await CreateVerificationRequestAsync();
        var wrongEmail = $"wrong-email-{Guid.NewGuid()}@example.com"; // Mismatched email

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={requestId}&Email={wrongEmail}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestStatus with mismatched email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("EMAIL_MISMATCH"));
    }

    [Test]
    public async Task RequestStatus_ExpiredRequest_ShouldReturnGone()
    {
        // Arrange - Create an expired verification request
        var (requestId, email) = await CreateExpiredVerificationRequestAsync();

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={requestId}&Email={email}");

        // Assert
        Assert.AreEqual(HttpStatusCode.Gone, response.StatusCode,
            "RequestStatus with expired request should return Gone (410)");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("REQUEST_EXPIRED"));
    }

    [Test]
    public async Task RequestStatus_AlreadyUsedRequest_ShouldReturnBadRequest()
    {
        // Arrange - Create and use a verification request
        var (requestId, email, code) = await CreateAndUseVerificationRequestAsync();

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={requestId}&Email={email}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestStatus with already used request should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("REQUEST_USED"));
    }

    [Test]
    public async Task RequestStatus_MissingRequestId_ShouldReturnBadRequest()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?Email={email}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestStatus with missing request ID should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task RequestStatus_MissingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var requestId = "123456789";

        // Act
        var response = await GetAsync($"/api/authentication/email-verification/request-status?RequestId={requestId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestStatus with missing email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    /// <summary>
    /// Helper method to create a verification request and return the request ID and email
    /// </summary>
    private async Task<(string requestId, string email)> CreateVerificationRequestAsync()
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        
        // Create a unique username and email
        var username = $"test-status-{Guid.NewGuid()}";
        var email = $"status-{Guid.NewGuid()}@example.com";
        
        // Create a hash for the password
        var hashedPassword = hasher.Hash("Password123!");
        
        // Create and add a new user
        var newUser = new API.Infrastructure.Database.Entities.User.User
        {
            Username = username,
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.hash,
            PasswordSalt = hashedPassword.salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        
        await userRepository.AddUserAsync(newUser);
        
        // Add an unverified email for the user
        var newEmail = new API.Infrastructure.Database.Entities.User.EmailAddress
        {
            User = newUser,
            Value = email,
            State = EmailState.PendingVerification,
            Type = EmailType.Primary
        };
        
        await userRepository.AddEmailAsync(newEmail);
        
        // Create a verification code
        var code = "123456";
        
        // Create a verification request
        var verificationRequest = new EmailVerificationRequest
        {
            Code = code,
            User = newUser,
            EmailAddress = newEmail,
            UserId = newUser.Id,
            EmailId = newEmail.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        
        await verificationRepository.AddEmailVerificationRequestAsync(verificationRequest);
        
        return (verificationRequest.Id.ToString(), email);
    }

    /// <summary>
    /// Helper method to create an expired verification request
    /// </summary>
    private async Task<(string requestId, string email)> CreateExpiredVerificationRequestAsync()
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        
        // Create a unique username and email
        var username = $"test-expired-status-{Guid.NewGuid()}";
        var email = $"expired-status-{Guid.NewGuid()}@example.com";
        
        // Create a hash for the password
        var hashedPassword = hasher.Hash("Password123!");
        
        // Create and add a new user
        var newUser = new API.Infrastructure.Database.Entities.User.User
        {
            Username = username,
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.hash,
            PasswordSalt = hashedPassword.salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        
        await userRepository.AddUserAsync(newUser);
        
        // Add an unverified email for the user
        var newEmail = new API.Infrastructure.Database.Entities.User.EmailAddress
        {
            User = newUser,
            Value = email,
            State = EmailState.PendingVerification,
            Type = EmailType.Primary
        };
        
        await userRepository.AddEmailAsync(newEmail);
        
        // Create a verification code
        var code = "123456";
        
        // Create an expired verification request
        var verificationRequest = new EmailVerificationRequest
        {
            Code = code,
            User = newUser,
            EmailAddress = newEmail,
            UserId = newUser.Id,
            EmailId = newEmail.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10), // Expired request
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        
        await verificationRepository.AddEmailVerificationRequestAsync(verificationRequest);
        
        return (verificationRequest.Id.ToString(), email);
    }

    /// <summary>
    /// Helper method to create and use a verification request
    /// </summary>
    private async Task<(string requestId, string email, string code)> CreateAndUseVerificationRequestAsync()
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        
        // Create a unique username and email
        var username = $"test-used-status-{Guid.NewGuid()}";
        var email = $"used-status-{Guid.NewGuid()}@example.com";
        
        // Create a hash for the password
        var hashedPassword = hasher.Hash("Password123!");
        
        // Create and add a new user
        var newUser = new API.Infrastructure.Database.Entities.User.User
        {
            Username = username,
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.hash,
            PasswordSalt = hashedPassword.salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.PendingVerification
        };
        
        await userRepository.AddUserAsync(newUser);
        
        // Add an unverified email for the user
        var newEmail = new API.Infrastructure.Database.Entities.User.EmailAddress
        {
            User = newUser,
            Value = email,
            State = EmailState.PendingVerification,
            Type = EmailType.Primary
        };
        
        await userRepository.AddEmailAsync(newEmail);
        
        // Create a verification code
        var code = "123456";
        
        // Create a verification request
        var verificationRequest = new EmailVerificationRequest
        {
            Code = code,
            User = newUser,
            EmailAddress = newEmail,
            UserId = newUser.Id,
            EmailId = newEmail.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            IsUsed = true // Mark as used
        };
        
        await verificationRepository.AddEmailVerificationRequestAsync(verificationRequest);
        
        return (verificationRequest.Id.ToString(), email, code);
    }
} 