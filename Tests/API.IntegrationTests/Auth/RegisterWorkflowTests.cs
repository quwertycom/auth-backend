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
using API.Contracts.Responses.Common;
namespace API.IntegrationTests.Auth;

public class RegisterWorkflowTests : TestBase
{
    [Fact]
    public async Task Register_WithValidRequest_ReturnsSuccess()
    {
        await ResetDatabase();
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
            PhoneNumber = "+1234567890",
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
            .Include(u => u.PhoneNumbers)
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

        // Assert phone number
        // Assert.Single(user.PhoneNumbers);
        // var phoneNumber = user.PhoneNumbers.Single();
        // Assert.Equal(request.PhoneNumber, phoneNumber.Phone);
        // Assert.Contains(user.PhoneNumbers, p => p.Type == PhoneType.Primary);
        // TODO: Add phone number assertion, currently not implemented in the database
        
        // Assert verification session
        Assert.Single(verificationSessions);
        var session = verificationSessions.Single();
        Assert.Equal(request.Email, session.Email?.Email);
        Assert.False(session.IsUsed);
        Assert.NotNull(session.Code);
        Assert.InRange(session.Code.Length, 8, 8);
        Assert.NotEqual(default(DateTime), session.CreatedAt);
    }

    [Fact]
    public async Task Register_WithEmptyRequest_ReturnsBadRequest()
    {
        await ResetDatabase();
        // Arrange
        var request = new RegisterRequest
        {
            Username = "",
            FirstName = "",
            LastName = "",
            Password = "",
            BirthDate = DateTime.UtcNow,
            Email = "",
            Gender = UserGender.Male,
        };

        // Act
        var response = await PostAsync("/api/auth/register", request);
        var responseObject = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_REQUEST", responseObject?.Status);
        Assert.Contains("Invalid request format", responseObject?.Message);

        // Assert no database changes
        var dbContext = GetRequiredService<AuthDbContext>();
        var users = await dbContext.Users.Where(u => u.Username == request.Username).ToListAsync();
        var verificationSessions = await dbContext.VerificationSessions.Where(vs => vs.Email != null && vs.Email.Email == request.Email).ToListAsync();
        var emails = await dbContext.UserEmails.Where(e => e.Email == request.Email).ToListAsync();
        var phoneNumbers = await dbContext.UserPhoneNumbers.Where(p => p.Phone == request.PhoneNumber).ToListAsync();
        Assert.Empty(users);
        Assert.Empty(verificationSessions);
        Assert.Empty(emails);
        Assert.Empty(phoneNumbers);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        await ResetDatabase();
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test12",
            LastName = "User",
            Password = "password",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            Gender = UserGender.Male,
            Email = "invalid-email",
        };

        // Act
        var response = await PostAsync("/api/auth/register", request);
        var responseObject = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_EMAIL", responseObject?.Status);
        Assert.Equal("Invalid email format.", responseObject?.Message);

        // Assert no database changes
        var dbContext = GetRequiredService<AuthDbContext>();
        var users = await dbContext.Users.Where(u => u.Username == request.Username).ToListAsync();
        var verificationSessions = await dbContext.VerificationSessions.Where(vs => vs.Email != null && vs.Email.Email == request.Email).ToListAsync();
        var emails = await dbContext.UserEmails.Where(e => e.Email == request.Email).ToListAsync();
        var phoneNumbers = await dbContext.UserPhoneNumbers.Where(p => p.Phone == request.PhoneNumber).ToListAsync();
        Assert.Empty(users);
        Assert.Empty(verificationSessions);
        Assert.Empty(emails);
        Assert.Empty(phoneNumbers);
    }

    [Fact]
    public async Task Register_WithInvalidUsername_ReturnsBadRequest()
    {
        await ResetDatabase();
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser@123",
            FirstName = "Test",
            LastName = "User",
            Password = "password",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            Gender = UserGender.Male,
            Email = "testuser@example.com",
            PhoneNumber = "+1234567890",
        };

        // Act
        var response = await PostAsync("/api/auth/register", request);
        var responseObject = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_USERNAME", responseObject?.Status);
        Assert.Equal("Invalid username format.", responseObject?.Message);

        // Assert no database changes
        var dbContext = GetRequiredService<AuthDbContext>();
        var users = await dbContext.Users.Where(u => u.Username == request.Username).ToListAsync();
        var verificationSessions = await dbContext.VerificationSessions.Where(vs => vs.Email != null && vs.Email.Email == request.Email).ToListAsync();
        var emails = await dbContext.UserEmails.Where(e => e.Email == request.Email).ToListAsync();
        var phoneNumbers = await dbContext.UserPhoneNumbers.Where(p => p.Phone == request.PhoneNumber).ToListAsync();
        Assert.Empty(users);
        Assert.Empty(verificationSessions);
        Assert.Empty(emails);
        Assert.Empty(phoneNumbers);
    }
}