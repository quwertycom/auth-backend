using System.Net;
using System.Net.Http.Json;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using NUnit.Framework;

namespace API.Tests.Functional.Features.User.Password.Reset;

[TestFixture]
public class RequestPasswordResetWorkflowTests : TestBase
{
    [Test]
    public async Task RequestPasswordReset_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/user/password/reset/request");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task RequestPasswordReset_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        // Create and add active email address
        var email = _generate.NewEmailAddress(
            value: "test@example.com",
            user: user,
            state: EmailState.Active
        );
        await userRepo.AddEmailAsync(email);

        var request = new RequestPasswordResetRequest
        {
            Email = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.Message.Should().Be("Password reset request sent");
    }

    [Test]
    public async Task RequestPasswordReset_WithValidUsername_ShouldReturnSuccess()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        // Add active email
        var email = _generate.NewEmailAddress(
            value: "test@example.com",
            user: user,
            state: EmailState.Active
        );
        await userRepo.AddEmailAsync(email);

        var request = new RequestPasswordResetRequest
        {
            Username = user.Username
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("SUCCESS");
        content.Message.Should().Be("Password reset request sent");
    }

    [Test]
    public async Task RequestPasswordReset_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestPasswordResetRequest
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("not found");
    }

    [Test]
    public async Task RequestPasswordReset_WithNonExistentUsername_ShouldReturnNotFound()
    {
        // Arrange
        var request = new RequestPasswordResetRequest
        {
            Username = "nonexistentuser"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("not found");
    }

    [Test]
    public async Task RequestPasswordReset_WithDisabledEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userRepo = GetRequiredService<IUserRepository>();
        var user = _generate.NewUser();
        await userRepo.AddUserAsync(user);

        // Add inactive email
        var email = _generate.NewEmailAddress(
            value: "inactive@example.com",
            user: user,
            state: EmailState.Disabled
        );
        await userRepo.AddEmailAsync(email);

        var request = new RequestPasswordResetRequest
        {
            Email = "inactive@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("not active");
    }

    [Test]
    public async Task RequestPasswordReset_WithBothEmailAndUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestPasswordResetRequest
        {
            Email = "test@example.com",
            Username = "testuser"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Validation Error");
        content.Details.Should().NotBeNull();
        content.Details!.Count.Should().Be(1);
        Console.WriteLine(content.Details.ToString());
        content.Details.Should().ContainKey("");
        content.Details[""].Should().HaveCount(1);
        content.Details[""][0].Should().Contain("Either Email or Username must be specified, but not both.");
    }

    [Test]
    public async Task RequestPasswordReset_WithNeitherEmailNorUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestPasswordResetRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Console.WriteLine(await response.Content.ReadAsStringAsync());

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Details.Should().NotBeNull();
        content.Details!.Count.Should().Be(2);
        content.Details.Should().ContainKey("");
        content.Details[""].Should().HaveCount(1);
        content.Details[""][0].Should().Contain("Either Email or Username must be specified, but not both.");
        content.Details.Should().ContainKey("username");
        content.Details["username"].Should().HaveCount(1);
        content.Details["username"][0].Should().Contain("Username is required!");
    }

    [Test]
    public async Task RequestPasswordReset_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestPasswordResetRequest
        {
            Email = "invalid-email"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Validation Error");
        content.Details.Should().NotBeNull();
        content.Details!.Count.Should().Be(1);
        content.Details.Should().ContainKey("email");
        content.Details["email"].Should().HaveCount(1);
        content.Details["email"][0].Should().Contain("Invalid email address!");
    }

    [Test]
    public async Task RequestPasswordReset_WithInvalidUsernameFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RequestPasswordResetRequest
        {
            Username = "invalid username!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/password/reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("ERROR");
        content.Message.Should().Contain("Validation Error");
        content.Details.Should().NotBeNull();
        content.Details!.Count.Should().Be(1);
        content.Details.Should().ContainKey("username");
        content.Details["username"].Should().HaveCount(1);
        content.Details["username"][0].Should().Contain("Username must contain only letters, numbers, and underscores!");
    }
}
