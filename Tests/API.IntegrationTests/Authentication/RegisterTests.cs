using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.Register.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using NUnit.Framework;

namespace API.IntegrationTests.Authentication;

[TestFixture]
public class RegisterTests : TestBase
{
    [Test]
    public async Task Register_Endpoint_Should_BeAccessible()
    {
        // Act: Send request to the register endpoint
        var response = await _client.GetAsync("/api/authentication/register");

        // Assert: The endpoint exists even if it returns method not allowed
        Assert.IsFalse(response.StatusCode == HttpStatusCode.NotFound,
            "Register endpoint should exist and not return 404 Not Found");
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode,
            "Register endpoint should return Method Not Allowed for GET requests");
    }

    [Test]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser_valid",
            FirstName = "Test",
            LastName = "User",
            Email = "testvalid@example.com",
            PhoneNumber = "+1234567890",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay,
            Password = "StrongPassword123!"
        };

        // Act
        var response = await PostAsync("/api/authentication/register", request);

        // Assert - strict check for OK status
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            $"Expected OK status code for valid registration, but got {response.StatusCode}");

        // Verify the success response
        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
        Assert.IsNotNull(content.RequestId, "Response should include a RequestId");
    }

    [Test]
    public async Task Register_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = $"testuser_invalid_email_{Guid.NewGuid()}",
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email-format", // Invalid email format
            Password = "Password123!",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay
        };

        // Act
        var response = await PostAsync("/api/authentication/register", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "Registration with invalid email format should return Bad Request");

        // Verify error response contains a message without requiring specific text
        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task Register_WithPasswordTooShort_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = $"testuser_short_password_{Guid.NewGuid()}",
            FirstName = "Test",
            LastName = "User",
            Email = $"short_password_{Guid.NewGuid()}@example.com",
            Password = "Short1!", // Password too short
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay
        };

        // Act
        var response = await PostAsync("/api/authentication/register", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "Registration with password too short should return Bad Request");

        // Verify error response contains a message without requiring specific text
        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task Register_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = null!, // Missing Username
            FirstName = "Test",
            LastName = "User",
            Email = $"missing_username_{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay
        };

        // Act
        var response = await PostAsync("/api/authentication/register", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "Registration with missing required fields should return Bad Request");

        // Verify error response contains a message
        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.IsNotNull(errorContent.Details, "Error details should not be null");
    }

    [Test]
    public async Task Register_WithAnyUsername_ShouldReturnConsistentResponse()
    {
        // Arrange - Using a test user
        var request = new RegisterRequest
        {
            Username = $"testuser_special_{Guid.NewGuid()}",
            FirstName = "Test",
            LastName = "User",
            Email = $"special_test_{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay
        };

        // Act
        var response = await PostAsync("/api/authentication/register", request);

        // Assert - we check that we either get success or a clear error
        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.BadRequest),
            "Registration should return either OK or BadRequest");

        // Verify that we can deserialize the response
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            Assert.IsNotNull(content, "Success response content should not be null");
        }
        else
        {
            var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.IsNotNull(errorContent, "Error response should not be null");
        }
    }
}