using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using NUnit.Framework;

namespace API.Tests.Functional.Authentication.EmailVerification;

[TestFixture]
public class RequestStatusWorkflowTests : TestBase
{
    [Test]
    public async Task RequestStatus_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/authentication/email-verification/request-status");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestStatus_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var (requestId, email, _) = await CreatePendingVerificationRequestAsync();

        var request = new RequestStatusRequest
        {
            RequestId = requestId,
            Email = email
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={requestId}&email={email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task RequestStatus_WithNonExistingRequest_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestStatusRequest
        {
            RequestId = "999999",
            Email = $"non-existing-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={request.RequestId}&email={request.Email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task RequestStatus_WithExpiredRequest_ShouldReturnGone()
    {
        // Arrange
        var (requestId, email, _) = await CreateExpiredVerificationRequestAsync();

        var request = new RequestStatusRequest
        {
            RequestId = requestId,
            Email = email
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={requestId}&email={email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Test]
    public async Task RequestStatus_WithUsedRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var (requestId, email, _) = await CreateUsedVerificationRequestAsync();

        var request = new RequestStatusRequest
        {
            RequestId = requestId,
            Email = email
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={requestId}&email={email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestStatus_WithInvalidRequestId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestStatusRequest
        {
            RequestId = "invalid-id",
            Email = $"test-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={request.RequestId}&email={request.Email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestStatus_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestStatusRequest
        {
            RequestId = "123456",
            Email = "invalid-email"
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={request.RequestId}&email={request.Email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestStatus_WithMissingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestStatusRequest
        {
            RequestId = "123456",
            Email = null!
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?requestId={request.RequestId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RequestStatus_WithMissingRequestId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestStatusRequest
        {
            RequestId = null!,
            Email = $"test-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await _client.GetAsync($"/api/authentication/email-verification/request-status?email={request.Email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<(string requestId, string email, string code)> CreatePendingVerificationRequestAsync()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var verificationRepo = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"pending-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var user = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.PendingVerification
        );
        await userRepo.AddUserAsync(user);

        var emailAddress = _generate.NewEmailAddress(
            user: user,
            value: email,
            state: EmailState.PendingVerification
        );
        await userRepo.AddEmailAsync(emailAddress);

        var verificationRequest = _generate.NewEmailVerificationRequest(
            code: "123456",
            user: user,
            emailAddress: emailAddress,
            expiresAt: DateTime.UtcNow.AddMinutes(10)
        );
        await verificationRepo.AddEmailVerificationRequestAsync(verificationRequest);

        return (verificationRequest.Id.ToString(), email, verificationRequest.Code);
    }

    private async Task<(string requestId, string email, string code)> CreateExpiredVerificationRequestAsync()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var verificationRepo = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"expired-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var user = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.PendingVerification
        );
        await userRepo.AddUserAsync(user);

        var emailAddress = _generate.NewEmailAddress(
            user: user,
            value: email,
            state: EmailState.PendingVerification
        );
        await userRepo.AddEmailAsync(emailAddress);

        var verificationRequest = _generate.NewEmailVerificationRequest(
            code: "123456",
            user: user,
            emailAddress: emailAddress,
            expiresAt: DateTime.UtcNow.AddMinutes(-10)
        );
        await verificationRepo.AddEmailVerificationRequestAsync(verificationRequest);

        return (verificationRequest.Id.ToString(), email, verificationRequest.Code);
    }

    private async Task<(string requestId, string email, string code)> CreateUsedVerificationRequestAsync()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var verificationRepo = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();

        var email = $"used-{Guid.NewGuid()}@example.com";
        var hashedPassword = hasher.Hash("Password123!");
        var user = _generate.NewUser(
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            state: UserState.PendingVerification
        );
        await userRepo.AddUserAsync(user);

        var emailAddress = _generate.NewEmailAddress(
            user: user,
            value: email,
            state: EmailState.PendingVerification
        );
        await userRepo.AddEmailAsync(emailAddress);

        var verificationRequest = _generate.NewEmailVerificationRequest(
            code: "123456",
            user: user,
            emailAddress: emailAddress,
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            isUsed: true
        );
        await verificationRepo.AddEmailVerificationRequestAsync(verificationRequest);

        return (verificationRequest.Id.ToString(), email, verificationRequest.Code);
    }
}
