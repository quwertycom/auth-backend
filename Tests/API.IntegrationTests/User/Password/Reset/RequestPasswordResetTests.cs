using System.Net;
using System.Net.Http.Json;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using NUnit.Framework;

namespace API.IntegrationTests.User.Password.Reset;

[TestFixture]
public class RequestPasswordResetTests : TestBase
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
        var username = $"resetusernamevalid{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}"; // Shorten username and remove hyphens
        var email = $"{username}@example.com";
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        var request = new RequestPasswordResetRequest { Username = username };

        // Act
        var response = await PostAsync("/api/user/password/reset/request", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "RequestPasswordReset with valid username should return OK");

        var content = await response.Content.ReadFromJsonAsync<RequestPasswordResetResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
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
}