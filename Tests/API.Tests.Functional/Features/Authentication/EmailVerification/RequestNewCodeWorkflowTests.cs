using System.Net;
using System.Net.Http.Json;
using System.Text;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Infrastructure.Security;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using Newtonsoft.Json;

namespace API.Tests.Functional.Authentication.EmailVerification;

[TestFixture]
public class RequestNewCodeWorkflowTests : TestBase
{
    [Test]
    public async Task RequestNewCode_Endpoint_Should_Be_Accessible()
    {
        var response = await _client.GetAsync("/api/authentication/email-verification/request-new-code");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task RequestNewCode_WithValidPendingEmail_ShouldReturnSuccess()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"pending-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.PendingVerification
        );
        await userRepo.AddUserAsync(existingUser);

        // Add email address with pending verification state
        var emailAddress = _generate.NewEmailAddress(
            user: existingUser,
            value: email,
            state: EmailState.PendingVerification
        );
        await userRepo.AddEmailAsync(emailAddress);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task RequestNewCode_WithNonExistingEmail_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestNewCodeRequest
        {
            Email = $"non-existing-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task RequestNewCode_WithAlreadyVerifiedEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"verified-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.Active
        );
        await userRepo.AddUserAsync(existingUser);

        // Add email address with active (verified) state
        var emailAddress = _generate.NewEmailAddress(
            user: existingUser,
            value: email,
            state: EmailState.Active
        );
        await userRepo.AddEmailAsync(emailAddress);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestNewCode_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestNewCodeRequest
        {
            Email = "invalid-email"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestNewCode_WithBlacklistedEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"blacklisted-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.Active
        );
        await userRepo.AddUserAsync(existingUser);

        var emailAddress = _generate.NewEmailAddress(
            user: existingUser,
            value: email,
            state: EmailState.Blacklisted
        );
        await userRepo.AddEmailAsync(emailAddress);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestNewCode_WithDisabledEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"disabled-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.Active
        );
        await userRepo.AddUserAsync(existingUser);

        var emailAddress = _generate.NewEmailAddress(
            user: existingUser,
            value: email,
            state: EmailState.Disabled
        );
        await userRepo.AddEmailAsync(emailAddress);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestNewCode_WithDeletedEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"deleted-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.Active
        );
        await userRepo.AddUserAsync(existingUser);

        var emailAddress = _generate.NewEmailAddress(
            user: existingUser,
            value: email,
            state: EmailState.Deleted
        );
        await userRepo.AddEmailAsync(emailAddress);

        var request = new RequestNewCodeRequest
        {
            Email = email
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestNewCode_WithMissingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestNewCodeRequest
        {
            Email = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/request-new-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}