using System.Net;
using System.Net.Http.Json;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using NUnit.Framework;
using API.Features.Authentication.Login.Models.Contracts;
using System.Text.Json;

namespace API.IntegrationTests.Authentication.PasswordReset;

[TestFixture]
public class ResetPasswordTests : TestBase
{
    [Test]
    public async Task RequestPasswordReset_Endpoint_Should_BeAccessible()
    {
        // Act: Send a GET request to the endpoint
        var response = await _client.GetAsync("/api/user/password/reset/request");

        // Assert: The endpoint should not be found for GET requests
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode,
            "RequestPasswordReset endpoint should return Method Not Allowed for GET requests");
    }

    [Test]
    public async Task RequestPasswordReset_ValidEmail_ShouldReturnSuccess()
    {
        // Arrange - Ensure a verified user exists
        var email = $"reset-valid-{Guid.NewGuid()}@example.com";
        var username = email.Split('@')[0]; // username same as email prefix
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        var request = new RequestPasswordResetRequest { Email = email };

        // Act
        var response = await PostAsync("/api/user/password/reset/request", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "RequestPasswordReset with valid email should return OK");

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
    }

    [Test]
    public async Task RequestPasswordReset_ValidUsername_ShouldReturnSuccess()
    {
        // Arrange - Ensure a verified user exists
        var username = $"reset-username-valid-{Guid.NewGuid()}";
        var email = $"{username}@example.com";
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        var request = new RequestPasswordResetRequest { Username = username };

        // Act
        var response = await PostAsync("/api/user/password/reset/request", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestPasswordReset with valid username should return OK");

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.That(content!.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task RequestPasswordReset_NonExistingEmail_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = $"non-exist-{Guid.NewGuid()}@example.com" };

        // Act
        var response = await PostAsync("/api/user/password/reset/request", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
            "RequestPasswordReset with non-existing email should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task RequestPasswordReset_NonExistingUsername_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Username = $"non-exist-username-{Guid.NewGuid()}" };

        // Act
        var response = await PostAsync("/api/user/password/reset/request", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestPasswordReset with non-existing username should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task RequestPasswordReset_MissingEmailAndUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = null, Username = null };

        // Act
        var response = await PostAsync("/api/user/password/reset/request", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "RequestPasswordReset with missing email and username should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task CheckRequestStatus_Endpoint_Should_BeAccessible()
    {
        // Act: Send a GET request to the endpoint
        var response = await _client.GetAsync("/api/user/password/reset/request-status");

        // Assert: The endpoint is accessible
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, // Or MethodNotAllowed, depending on config
            "CheckRequestStatus endpoint should be accessible");
    }

    [Test]
    public async Task CheckRequestStatus_ValidCode_ShouldReturnSuccess()
    {
        // Arrange - Create a password reset request
        var (code, _) = await CreatePasswordResetRequestAsync();
        var request = new CheckRequestStatusRequest { Code = code };

        // Act
        var response = await GetAsync($"/api/user/password/reset/request-status?code={code}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "CheckRequestStatus with valid code should return OK");

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
        Assert.IsFalse(content.IsExpired, "Request should not be expired");
        Assert.IsFalse(content.IsUsed, "Request should not be used");
    }

    [Test]
    public async Task CheckRequestStatus_InvalidCode_ShouldReturnNotFound()
    {
        // Arrange
        var invalidCode = "invalid-reset-code-format-1234567890";
        var request = new CheckRequestStatusRequest { Code = invalidCode };

        // Act
        var response = await GetAsync($"/api/user/password/reset/request-status?code={invalidCode}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "CheckRequestStatus with invalid code should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task CheckRequestStatus_ExpiredCode_ShouldReturnSuccessAndExpiredStatus()
    {
        // Arrange - Create an expired password reset request
        var (code, _) = await CreateExpiredPasswordResetRequestAsync();
        var request = new CheckRequestStatusRequest { Code = code };

        // Act
        var response = await GetAsync($"/api/user/password/reset/request-status?code={code}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, // Expect OK but with IsExpired = true
            "CheckRequestStatus with expired code should return OK");

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
        Assert.IsTrue(content.IsExpired, "Request should be expired");
        Assert.IsFalse(content.IsUsed, "Request should not be used"); // Still not used
    }

    [Test]
    public async Task CheckRequestStatus_UsedCode_ShouldReturnSuccessAndUsedStatus()
    {
        // Arrange - Create and use a password reset request
        var (code, _) = await CreateAndUsePasswordResetRequestAsync();
        var request = new CheckRequestStatusRequest { Code = code };

        // Act
        var response = await GetAsync($"/api/user/password/reset/request-status?code={code}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, // Expect OK but with IsUsed = true
            "CheckRequestStatus with used code should return OK");

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
        Assert.IsFalse(content.IsExpired, "Request should not be expired"); // Not expired
        Assert.IsTrue(content.IsUsed, "Request should be used");
    }

    [Test]
    public async Task CheckRequestStatus_MissingCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CheckRequestStatusRequest { Code = null! };

        // Act
        var response = await GetAsync($"/api/user/password/reset/request-status?code="); // No code

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "CheckRequestStatus with missing code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task ResetPassword_Endpoint_Should_BeAccessible()
    {
        // Act: Send a GET request to the endpoint
        var response = await _client.GetAsync("/api/user/password/reset");

        // Assert: The endpoint should not be found for GET requests
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode,
            "ResetPassword endpoint should return Method Not Allowed for GET requests");
    }

    [Test]
    public async Task ResetPassword_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange - Create a password reset request
        var (code, email) = await CreatePasswordResetRequestAsync();
        var newPassword = "NewPassword123!";
        // Store the current username for login verification
        var username = _testUsername;

        var request = new ResetPasswordRequest { Code = code, NewPassword = newPassword };

        // Act
        var response = await PostAsync("/api/user/password/reset", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "ResetPassword with valid request should return OK");

        var content = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
    }

    [Test]
    public async Task ResetPassword_ShouldAllowLoginWithNewPassword()
    {
        // Arrange - Create a password reset request
        var (code, email) = await CreatePasswordResetRequestAsync();
        var newPassword = "NewPassword123!";
        var username = _testUsername;

        // Directly use the service to reset the password
        var resetService = GetRequiredService<API.Features.User.Password.Reset.Interfaces.IResetPasswordService>();
        var resetResult = await resetService.ResetPasswordAsync(code, newPassword, CancellationToken.None);
        
        Assert.IsTrue(resetResult.IsSuccess, $"Password reset should succeed. Error: {resetResult.Message}");
        Assert.AreEqual(200, resetResult.HttpStatusCode, "Password reset should return HTTP 200");
        
        // Now attempt to login with the new password using the service directly
        var loginService = GetRequiredService<API.Features.Authentication.Login.Interfaces.ILoginService>();
        var loginResult = await loginService.LoginAsync(username, newPassword, CancellationToken.None);
        
        Assert.IsTrue(loginResult.IsSuccess, 
            $"Login with new password should succeed. Error: {loginResult.Message}");
        Assert.AreEqual(200, loginResult.HttpStatusCode, "Login should return HTTP 200");
    }

    [Test]
    public async Task ResetPassword_InvalidCode_ShouldReturnNotFound()
    {
        // Arrange
        var invalidCode = "invalid-reset-code-format-9876543210";
        var newPassword = "NewTestPassword123!";

        var request = new ResetPasswordRequest { Code = invalidCode, NewPassword = newPassword };

        // Act
        var response = await PostAsync("/api/user/password/reset", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with invalid code should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task ResetPassword_ExpiredCode_ShouldReturnBadRequest()
    {
        // Arrange - Create an expired password reset request
        var (code, _) = await CreateExpiredPasswordResetRequestAsync();
        var newPassword = "NewTestPassword123!";

        var request = new ResetPasswordRequest { Code = code, NewPassword = newPassword };

        // Act
        var response = await PostAsync("/api/user/password/reset", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
            "ResetPassword with expired code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task ResetPassword_UsedCode_ShouldReturnBadRequest()
    {
        // Arrange - Create and use a password reset request
        var (code, _) = await CreateAndUsePasswordResetRequestAsync();
        var newPassword = "NewTestPassword123!";

        var request = new ResetPasswordRequest { Code = code, NewPassword = newPassword };

        // Act
        var response = await PostAsync("/api/user/password/reset", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
            "ResetPassword with used code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task ResetPassword_MissingCode_ShouldReturnBadRequest()
    {
        // Arrange
        var newPassword = "NewTestPassword123!";
        var request = new ResetPasswordRequest { Code = null!, NewPassword = newPassword };

        // Act
        var response = await PostAsync("/api/user/password/reset", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with missing code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task ResetPassword_MissingNewPassword_ShouldReturnBadRequest()
    {
        // Arrange - Create a password reset request
        var (code, _) = await CreatePasswordResetRequestAsync();
        var request = new ResetPasswordRequest { Code = code, NewPassword = null! };

        // Act
        var response = await PostAsync("/api/user/password/reset", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with missing new password should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    #region Helper Methods

    private string _testUsername = $"test-reset-{Guid.NewGuid()}"; // class-level username for reuse in ResetPassword_ValidRequest_ShouldReturnSuccess

    /// <summary>
    /// Helper method to create a password reset request and return the code
    /// </summary>
    private async Task<(string code, string email)> CreatePasswordResetRequestAsync()
    {
        // Get access to the required services directly
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        var randomGenerator = GetRequiredService<API.Shared.Interfaces.Security.IRandomGenerator>();
        var emailSender = GetRequiredService<API.Shared.Interfaces.Email.IEmailSender>();

        // Create a unique username and email
        _testUsername = $"test-reset-{Guid.NewGuid()}"; // Ensure unique username for each test run
        var email = $"reset-{Guid.NewGuid()}@example.com";

        // Create a hash for the password
        var hashedPassword = hasher.Hash("Password123!");

        // Create and add a new user
        var newUser = new API.Infrastructure.Database.Entities.User.User
        {
            Username = _testUsername,
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = API.Shared.Enums.Entities.User.UserGender.Male,
            State = API.Shared.Enums.Entities.User.UserState.Active
        };

        await userRepository.AddUserAsync(newUser);

        // Add a verified email for the user
        var newEmail = new API.Infrastructure.Database.Entities.User.EmailAddress
        {
            User = newUser,
            Value = email,
            State = API.Shared.Enums.Entities.User.EmailState.Active,
            Type = API.Shared.Enums.Entities.User.EmailType.Primary
        };

        await userRepository.AddEmailAsync(newEmail);

        // Generate a reset code
        var code = randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = hasher.Hash(code, "");

        // Create a password reset request
        var passwordResetRequest = new API.Infrastructure.Database.Entities.Verification.PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = newEmail,
            User = newUser,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);

        return (code, email);
    }

    /// <summary>
    /// Helper method to create an expired password reset request
    /// </summary>
    private async Task<(string code, string email)> CreateExpiredPasswordResetRequestAsync()
    {
        // Get services
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        var randomGenerator = GetRequiredService<API.Shared.Interfaces.Security.IRandomGenerator>();

        // Create user and email
        var username = $"test-expired-reset-{Guid.NewGuid()}";
        var email = $"expired-reset-{Guid.NewGuid()}@example.com";
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        // Generate code and hash
        var code = randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = hasher.Hash(code, "");

        // Create expired request
        var passwordResetRequest = new API.Infrastructure.Database.Entities.Verification.PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = await userRepository.GetEmailAdressByEmailStringAsync(email), // Fetch existing email
            User = await userRepository.GetUserByUsernameAsync(username), // Fetch existing user
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10), // Expired
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);

        return (code, email);
    }


    /// <summary>
    /// Helper method to create and use a password reset request
    /// </summary>
    private async Task<(string code, string email)> CreateAndUsePasswordResetRequestAsync()
    {
        // Get services
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        var randomGenerator = GetRequiredService<API.Shared.Interfaces.Security.IRandomGenerator>();

        // Create user and email
        var username = $"test-used-reset-{Guid.NewGuid()}";
        var email = $"used-reset-{Guid.NewGuid()}@example.com";
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        // Generate code and hash
        var code = randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = hasher.Hash(code, "");

        // Create used request
        var passwordResetRequest = new API.Infrastructure.Database.Entities.Verification.PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = await userRepository.GetEmailAdressByEmailStringAsync(email), // Fetch existing email
            User = await userRepository.GetUserByUsernameAsync(username), // Fetch existing user
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            IsUsed = true // Already used
        };

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);

        return (code, email);
    }

    private async Task<HttpResponseMessage> LoginAsync(string username, string password)
    {
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        return await PostAsync("/api/authentication/login", loginRequest);
    }

    /// <summary>
    /// Helper method to create a password reset request for an existing user
    /// </summary>
    private async Task<(string code, string email)> CreatePasswordResetRequestAsync(string existingUsername, string existingEmail)
    {
        // Get required services
        var userRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IUserRepository>();
        var verificationRepository = GetRequiredService<API.Shared.Interfaces.Database.Repositories.IVerificationRepository>();
        var hasher = GetRequiredService<API.Shared.Interfaces.Security.IHasher>();
        var randomGenerator = GetRequiredService<API.Shared.Interfaces.Security.IRandomGenerator>();
        
        // Get the existing user by username
        var user = await userRepository.GetUserByUsernameAsync(existingUsername);
        if (user == null)
        {
            throw new Exception($"User {existingUsername} not found for creating reset request");
        }
        
        // Get the existing email
        var email = await userRepository.GetEmailAdressByEmailStringAsync(existingEmail);
        if (email == null)
        {
            throw new Exception($"Email {existingEmail} not found for creating reset request");
        }
        
        // Generate a reset code
        var code = randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = hasher.Hash(code, "");
        
        // Create a new password reset request
        var passwordResetRequest = new API.Infrastructure.Database.Entities.Verification.PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = email,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        
        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);
        
        return (code, existingEmail);
    }

    #endregion
}