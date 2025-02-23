using API.Models;
using API.Common.Enums;
using API.Contracts.Requests.Auth;
using System.Net;
using System.Net.Http.Json;
using API.Contracts.Responses.Auth;
using System.Text.Json;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.TypeMapping;
namespace API.IntegrationTests.Auth;

public class RegisterWorkflowTests : TestBase
{
    [Fact]
    public async Task Register_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Password = "password",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            Gender = UserGender.Male,
            Email = "testuser@example.com",
        };

        // Act
        var response = await PostAsync("/api/auth/register", request);
        var responseObject = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        
        // Get the DbContext from the service provider
        var dbContext = GetRequiredService<AuthDbContext>();

        // Query VerificationSessions to check if a session was created
        var verificationSessions = await dbContext.VerificationSessions
            .Where(vs => vs.Email != null && vs.Email.Email == request.Email)
            .Include(vs => vs.Email)
            .ToListAsync();
        
        var users = await dbContext.Users
            .Where(u => u.Username == request.Username)
            .Include(u => u.EmailAddresses)
            .ToListAsync();

        // Assert Response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseObject);
        Assert.Equal("SUCCESS", responseObject.Status);
        Assert.Equal("OTP has been sent to your email. Please verify your email and login.", responseObject.Message);
        Assert.NotEqual(0, responseObject.VerificationSessionID);

        // Assert user
        Assert.Single(users);
        var user = users.Single();
        Assert.Equal(request.Username, user.Username);
        Assert.Equal(request.FirstName, user.FirstName);
        Assert.Equal(request.LastName, user.LastName);
        Assert.Equal(request.Email, user.EmailAddresses.First().Email);
        Assert.Equal(request.BirthDate, user.BirthDate);
        Assert.Equal(request.Gender, user.Gender);

        // Assert email address
        Assert.Single(user.EmailAddresses);
        var emailAddress = user.EmailAddresses.Single();
        Assert.Equal(request.Email, emailAddress.Email);
        Assert.Contains(user.EmailAddresses, e => e.Type == EmailType.Primary);
        
        // Assert verification session
        Assert.Single(verificationSessions);
        var session = verificationSessions.Single();
        Assert.Equal(request.Email, session.Email?.Email);
        Assert.False(session.IsUsed);
        Assert.NotNull(session.Code);
        Assert.InRange(session.Code.Length, 8, 8);
        Assert.NotEqual(default(DateTime), session.CreatedAt);
    }
}
