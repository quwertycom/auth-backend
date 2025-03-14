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
public class CheckRequestStatusWorkflowTests : TestBase
{
    private readonly IHasher _hasher;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IRandomGenerator _randomGenerator;

    public CheckRequestStatusWorkflowTests()
    {
        _hasher = GetRequiredService<IHasher>();
        _verificationRepository = GetRequiredService<IVerificationRepository>();
        _randomGenerator = GetRequiredService<IRandomGenerator>();
    }

    [Test]
    public async Task CheckRequestStatus_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/user/password/reset/request-status");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CheckRequestStatus_WithValidCode_ShouldReturnSuccess()
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

        // Act
        var response = await _client.GetAsync($"/api/user/password/reset/request-status?code={code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.IsExpired.Should().BeFalse();
        content.IsUsed.Should().BeFalse();
    }

    [Test]
    public async Task CheckRequestStatus_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidCode = "invalid-code";

        // Act
        var response = await _client.GetAsync($"/api/user/password/reset/request-status?code={invalidCode}");

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
    public async Task CheckRequestStatus_WithExpiredCode_ShouldReturnSuccessWithExpiredStatus()
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
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // Expired
        };
        await _verificationRepository.AddPasswordResetRequestAsync(resetRequest);

        // Act
        var response = await _client.GetAsync($"/api/user/password/reset/request-status?code={code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.IsExpired.Should().BeTrue();
        content.IsUsed.Should().BeFalse();
    }

    [Test]
    public async Task CheckRequestStatus_WithUsedCode_ShouldReturnSuccessWithUsedStatus()
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

        // Act
        var response = await _client.GetAsync($"/api/user/password/reset/request-status?code={code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.IsExpired.Should().BeFalse();
        content.IsUsed.Should().BeTrue();
    }

    [Test]
    public async Task CheckRequestStatus_WithNonExistentCode_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentCode = _randomGenerator.GenerateAlphanumericCode(64);

        // Act
        var response = await _client.GetAsync($"/api/user/password/reset/request-status?code={nonExistentCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Request not found");
    }
}
