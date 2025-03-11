using API.IntegrationTests; // Ensure namespace matches your project
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using API.Features.Authentication.Login.Models.Contracts;

namespace API.IntegrationTests.User.Password.Reset;

[TestFixture]
public abstract class ResetPasswordTestBase : TestBase
{
    #region Helper Methods

    protected string _testUsername = $"test-reset-{Guid.NewGuid()}"; // class-level username for reuse in ResetPassword_ValidRequest_ShouldReturnSuccess

    /// <summary>
    /// Helper method to create a password reset request and return the code
    /// </summary>
    protected async Task<(string code, string email)> CreatePasswordResetRequestAsync()
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
    protected async Task<(string code, string email)> CreateExpiredPasswordResetRequestAsync()
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

        var user = await userRepository.GetUserByUsernameAsync(username);
        if (user == null)
        {
            throw new Exception($"User with username '{username}' not found.");
        }

        var emailAddress = user.EmailAddresses.FirstOrDefault();
        if (emailAddress == null)
        {
            throw new Exception($"Email address for user '{username}' not found.");
        }

        // Generate code and hash
        var code = randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = hasher.Hash(code, "");

        // Create expired request
        var passwordResetRequest = new API.Infrastructure.Database.Entities.Verification.PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = emailAddress,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10), // Expired
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);

        return (code, email);
    }


    /// <summary>
    /// Helper method to create and use a password reset request
    /// </summary>
    protected async Task<(string code, string email)> CreateAndUsePasswordResetRequestAsync()
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
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
        var passwordResetRequest = new API.Infrastructure.Database.Entities.Verification.PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = await userRepository.GetEmailAdressByEmailStringAsync(email),
            User = await userRepository.GetUserByUsernameAsync(username),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            IsUsed = true // Already used
        };
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);

        return (code, email);
    }

    protected async Task<HttpResponseMessage> LoginAsync(string username, string password)
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
    protected async Task<(string code, string email)> CreatePasswordResetRequestAsync(string existingUsername, string existingEmail)
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