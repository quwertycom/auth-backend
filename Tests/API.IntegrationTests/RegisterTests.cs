using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.Register.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;

namespace API.IntegrationTests;

[TestFixture]
public class RegisterTests : TestBase
{
    [Test]
    public async Task Register_Endpoint_Should_BeAccessible()
    {
        // Act: Simply check if the endpoint exists and responds (status code could be 400 if validation fails)
        var response = await _client.GetAsync("/api/authentication/register");

        // Assert: The endpoint exists even if it returns method not allowed
        Assert.IsFalse(response.StatusCode == HttpStatusCode.NotFound, "Register endpoint should exist");
    }

    [Test]
    [Ignore("Registration is not fully implemented yet")]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PhoneNumber = "+1234567890",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay,
            Password = "StrongPassword123!"
        };

        // Act
        var response = await PostAsync("/api/authentication/register", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.IsNotNull(content);
        Assert.AreEqual("SUCCESS", content!.Status);
        Assert.IsNotNull(content.EmailVerificationSessionId);
    }

    [Test]
    [Ignore("Registration is not fully implemented yet")]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange - First registration
        var firstRequest = new RegisterRequest
        {
            Username = "firstuser",
            FirstName = "First",
            LastName = "User",
            Email = "duplicate@example.com",
            PhoneNumber = "+1234567890",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.PreferNotToSay,
            Password = "StrongPassword123!"
        };

        await PostAsync("/api/authentication/register", firstRequest);

        // Arrange - Second registration with same email
        var secondRequest = new RegisterRequest
        {
            Username = "seconduser",
            FirstName = "Second",
            LastName = "User",
            Email = "duplicate@example.com", // Same email
            PhoneNumber = "+9876543210",
            BirthDate = new DateTime(1992, 2, 2),
            Gender = UserGender.PreferNotToSay,
            Password = "StrongPassword456!"
        };

        // Act
        var response = await PostAsync("/api/authentication/register", secondRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(content);
        Assert.AreEqual("EMAIL_EXISTS", content!.Status);
    }
}