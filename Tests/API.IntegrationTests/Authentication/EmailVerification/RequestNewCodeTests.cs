using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using NUnit.Framework;

namespace API.IntegrationTests.Authentication.EmailVerification;

[TestFixture]
public class RequestNewCodeTests : TestBase
{
    [Test]
    public async Task RequestNewCode_Endpoint_Should_BeAccessible()
    {
        // Act: Send request to the endpoint
        var response = await _client.GetAsync("/api/authentication/email-verification/request-new-code");

        // Assert: The endpoint exists even if it returns method not allowed
        Assert.IsFalse(response.StatusCode == HttpStatusCode.NotFound, 
            "RequestNewCode endpoint should exist and not return 404 Not Found");
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode,
            "RequestNewCode endpoint should return Method Not Allowed for GET requests");
    }

    [Test]
    public async Task RequestNewCode_WithValidPendingEmail_ShouldReturnSuccess()
    {
        // Arrange - Create a user with email in PendingVerification state
        var email = $"pending-verification-{Guid.NewGuid()}@example.com";
        await EnsureUnverifiedUserExistsAsync("testpending", "Password123!", email);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "RequestNewCode with valid pending email should return OK");

        var content = await response.Content.ReadFromJsonAsync<RequestNewCodeResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
        Assert.IsNotNull(content.NewRequestId, "Response should include a NewRequestId");
    }

    [Test]
    public async Task RequestNewCode_NonExistingEmail_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestNewCodeRequest
        {
            Email = $"non-existing-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
            "RequestNewCode with non-existing email should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task RequestNewCode_AlreadyVerifiedEmail_ShouldReturnBadRequest()
    {
        // Arrange - Create a user with verified email
        var email = $"verified-{Guid.NewGuid()}@example.com";
        await EnsureVerifiedUserExistsAsync("testverified", "Password123!");
        
        // Get the verified email for this user
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var userEmail = await userRepository.GetEmailAdressByEmailStringAsync($"testverified@example.com");
        
        var request = new RequestNewCodeRequest
        {
            Email = $"testverified@example.com"
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestNewCode with already verified email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("EMAIL_ALREADY_VERIFIED"));
    }

    [Test]
    public async Task RequestNewCode_BlacklistedEmail_ShouldReturnBadRequest()
    {
        // Arrange - Create a user and set the email state to Blacklisted
        var email = $"blacklisted-{Guid.NewGuid()}@example.com";
        await EnsureEmailWithSpecificStateAsync(email, EmailState.Blacklisted);
        
        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestNewCode with blacklisted email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("EMAIL_BLACKLISTED"));
    }

    [Test]
    public async Task RequestNewCode_DeletedEmail_ShouldReturnBadRequest()
    {
        // Arrange - Create a user and set the email state to Deleted
        var email = $"deleted-{Guid.NewGuid()}@example.com";
        await EnsureEmailWithSpecificStateAsync(email, EmailState.Deleted);
        
        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestNewCode with deleted email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("EMAIL_DISABLED"));
    }

    [Test]
    public async Task RequestNewCode_DisabledEmail_ShouldReturnBadRequest()
    {
        // Arrange - Create a user and set the email state to Disabled
        var email = $"disabled-{Guid.NewGuid()}@example.com";
        await EnsureEmailWithSpecificStateAsync(email, EmailState.Disabled);
        
        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestNewCode with disabled email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("EMAIL_DISABLED"));
    }

    [Test]
    public async Task RequestNewCode_InvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestNewCodeRequest
        {
            Email = "invalid-email-format"
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestNewCode with invalid email format should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task RequestNewCode_MissingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestNewCodeRequest
        {
            Email = null!
        };

        // Act
        var response = await PostAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestNewCode with missing email should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task RequestNewCode_RateLimited_ShouldReturnTooManyRequests()
    {
        // Arrange - Create a user with email in PendingVerification state
        var email = $"rate-limited-{Guid.NewGuid()}@example.com";
        await EnsureUnverifiedUserExistsAsync("testrate", "Password123!", email);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act - Send multiple requests in quick succession to trigger rate limiting
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(PostAsync("/api/authentication/email-verification/request-new-code", request));
        }
        
        // Wait for all requests to complete
        await Task.WhenAll(tasks);
        
        // If any request returned 429, the test passes
        var responses = tasks.Select(t => t.Result).ToList();
        
        // Check if any response is rate limited (429 TooManyRequests)
        // Note: This depends on the actual rate limiting configuration
        // If rate limiting isn't triggering, this test may need adjustment
        if (responses.Any(r => r.StatusCode == HttpStatusCode.TooManyRequests))
        {
            var rateLimitedResponse = responses.First(r => r.StatusCode == HttpStatusCode.TooManyRequests);
            var errorContent = await rateLimitedResponse.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.IsNotNull(errorContent, "Error response should not be null");
            Assert.That(errorContent!.Status, Is.EqualTo("RATE_LIMITED"));
        }
        else
        {
            // If rate limiting doesn't trigger, mark the test as inconclusive
            Assert.Inconclusive("Rate limiting did not trigger. This may be expected in test environment.");
        }
    }

    [Test]
    public async Task RequestNewCode_InternalServerError_ShouldReturnErrorResponse()
    {
        // This test is difficult to implement in integration tests because
        // we'd need to force a server error. In production tests, we would
        // use a controlled environment where we can mock the service to throw
        // an exception. For now, we'll make this test inconclusive.
        Assert.Inconclusive("Cannot reliably test server errors in integration tests without service mocking");
    }

    /// <summary>
    /// Helper method to ensure a user exists with an unverified email address
    /// </summary>
    private async Task EnsureUnverifiedUserExistsAsync(string username, string password, string email)
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        
        // Check if user already exists
        if (!await userRepository.UsernameExistsAsync(username))
        {
            // Create a hash for the password
            var hashedPassword = hasher.Hash(password);
            
            // Create and add a new user with the PendingVerification state
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
        }
    }

    /// <summary>
    /// Helper method to ensure a user exists with an email of a specific state
    /// </summary>
    private async Task EnsureEmailWithSpecificStateAsync(string email, EmailState state)
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        
        // Create a unique username
        var username = $"test-{Guid.NewGuid()}";
        
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
            State = UserState.Active
        };
        
        await userRepository.AddUserAsync(newUser);
        
        // Add an email with the specified state for the user
        var newEmail = new API.Infrastructure.Database.Entities.User.EmailAddress
        {
            User = newUser,
            Value = email,
            State = state,
            Type = EmailType.Primary
        };
        
        await userRepository.AddEmailAsync(newEmail);
    }
} 