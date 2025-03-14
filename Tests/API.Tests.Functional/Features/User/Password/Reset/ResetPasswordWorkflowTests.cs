using System.Net;
using System.Net.Http.Json;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using NUnit.Framework;

namespace API.Tests.Functional.Features.User.Password.Reset;

[TestFixture]
public class ResetPasswordWorkflowTests : TestBase
{
    private readonly IHasher _hasher;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IRandomGenerator _randomGenerator;

    public ResetPasswordWorkflowTests()
    {
        _hasher = GetRequiredService<IHasher>();
        _verificationRepository = GetRequiredService<IVerificationRepository>();
        _randomGenerator = GetRequiredService<IRandomGenerator>();
    }

    [Test]
    public async Task ResetPassword_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/user/password/reset");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task ResetPassword_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        var email = _generate.NewEmailAddress(
            value: "test@example.com",
            user: user,
            state: EmailState.Active
        );
        await userRepo.AddEmailAsync(email);

        var code = _randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = _hasher.Hash(code, "");

        var resetRequest = new PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = email,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };
        await _verificationRepository.AddPasswordResetRequestAsync(resetRequest);

        var request = new ResetPasswordRequest
        {
            Code = code,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.Message.Should().Be("Password reset successfully");
    }

    [Test]
    public async Task ResetPassword_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Code = "invalid-code",
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Validation Error");
        content.Details.Should().NotBeNull();
        content.Details!.Count.Should().Be(1);
        content.Details.Should().ContainKey("code");
        content.Details["code"].Should().HaveCount(1);
        content.Details["code"][0].Should().Contain("Code must be 64 characters long!");
    }

    [Test]
    public async Task ResetPassword_WithExpiredCode_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        var email = _generate.NewEmailAddress(
            value: "test@example.com",
            user: user,
            state: EmailState.Active
        );
        await userRepo.AddEmailAsync(email);

        var code = _randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = _hasher.Hash(code, "");

        var resetRequest = new PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = email,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired
            IsUsed = false
        };
        await _verificationRepository.AddPasswordResetRequestAsync(resetRequest);

        var request = new ResetPasswordRequest
        {
            Code = code,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Request expired");
    }

    [Test]
    public async Task ResetPassword_WithUsedCode_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        var email = _generate.NewEmailAddress(
            value: "test@example.com",
            user: user,
            state: EmailState.Active
        );
        await userRepo.AddEmailAsync(email);

        var code = _randomGenerator.GenerateAlphanumericCode(64);
        var codeHash = _hasher.Hash(code, "");

        var resetRequest = new PasswordResetRequest
        {
            CodeHash = codeHash.Hash,
            EmailAddress = email,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = true // Marked as used
        };
        await _verificationRepository.AddPasswordResetRequestAsync(resetRequest);

        var request = new ResetPasswordRequest
        {
            Code = code,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Request already used");
    }

    [Test]
    public async Task ResetPassword_WithNonExistentCode_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentCode = _randomGenerator.GenerateAlphanumericCode(64);
        var request = new ResetPasswordRequest
        {
            Code = nonExistentCode,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Request not found");
    }
}
