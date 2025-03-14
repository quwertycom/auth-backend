using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using NUnit.Framework;

namespace API.Tests.Functional.Authentication.EmailVerification;

[TestFixture]
public class VerifyEmailWorkflowTests : TestBase
{
    [Test]
    public async Task VerifyEmail_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/authentication/email-verification/verify");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task VerifyEmail_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var (requestId, code) = await CreatePendingVerificationRequestAsync();
        var request = new VerifyEmailRequest
        {
            RequestId = requestId,
            Code = code
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task VerifyEmail_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var (requestId, _) = await CreatePendingVerificationRequestAsync();
        var request = new VerifyEmailRequest
        {
            RequestId = requestId,
            Code = "wrong-code"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task VerifyEmail_WithExpiredRequest_ShouldReturnGone()
    {
        // Arrange
        var (requestId, code) = await CreateExpiredVerificationRequestAsync();
        var request = new VerifyEmailRequest
        {
            RequestId = requestId,
            Code = code
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Test]
    public async Task VerifyEmail_WithUsedRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var (requestId, code) = await CreateUsedVerificationRequestAsync();
        var request = new VerifyEmailRequest
        {
            RequestId = requestId,
            Code = code
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task VerifyEmail_WithInvalidRequestId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            RequestId = "invalid-id",
            Code = "123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task VerifyEmail_WithMissingCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            RequestId = "123456",
            Code = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task VerifyEmail_WithMissingRequestId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            RequestId = null!,
            Code = "123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/email-verification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<(string requestId, string code)> CreatePendingVerificationRequestAsync()
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

        return (verificationRequest.Id.ToString(), verificationRequest.Code);
    }

    private async Task<(string requestId, string code)> CreateExpiredVerificationRequestAsync()
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

        return (verificationRequest.Id.ToString(), verificationRequest.Code);
    }

    private async Task<(string requestId, string code)> CreateUsedVerificationRequestAsync()
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

        return (verificationRequest.Id.ToString(), verificationRequest.Code);
    }
}
