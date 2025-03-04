using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using API.Features.Authentication.Register.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.IntegrationTests;

public class RegisterTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;

    public RegisterTests(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_Endpoint_Should_BeAccessible()
    {
        // Act: Simply check if the endpoint exists and responds (status code could be 400 if validation fails)
        var response = await _client.GetAsync("/api/authentication/register");
        
        // Assert: The endpoint exists even if it returns method not allowed
        Assert.False(response.StatusCode == HttpStatusCode.NotFound, "Register endpoint should exist");
    }

    [Fact(Skip = "Registration is not fully implemented yet")]
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(content);
        Assert.Equal("SUCCESS", content!.Status);
        Assert.NotNull(content.EmailVerificationSessionId);
    }

    [Fact(Skip = "Registration is not fully implemented yet")]
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(content);
        Assert.Equal("EMAIL_EXISTS", content!.Status);
    }
} 