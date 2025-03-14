using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.Register.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using API.Infrastructure.Database.Entities.Verification;
using System.Text.Json.Serialization;
using System.Text.Json;
using API.Shared.Interfaces.Security;
using API.Infrastructure.Database.Entities.User;

namespace API.Tests.Functional.Authentication;

[TestFixture]
public class RegisterWorkflowTests : TestBase
{
    private IUserRepository? _userRepository;

    [SetUp]
    public void Setup()
    {
        _userRepository = GetRequiredService<IUserRepository>();
    }

    [Test]
    public async Task Register_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/authentication/register");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task Register_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        var request = new RegisterRequest
        {
            Username = "testusertest1",
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test1.com",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("SUCCESS");
        responseBody.Should().Contain("requestId");
    }

    [Test]
    public async Task Register_WithEmptyFields_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "",
            FirstName = "",
            LastName = "",
            Email = "",
            BirthDate = default,
            Gender = default,
            Password = "",
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_WithMaxLengthFields_ShouldReturnSuccess()
    {
        var request = new RegisterRequest
        {
            Username = new string('a', 32),
            FirstName = new string('a', 128),
            LastName = new string('a', 128),
            Email = $"{new string('a', 240)}@test.com",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Register_WithInvalidPhoneNumber_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "testusertest2",
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test2.com",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            Password = "Password123!",
            PhoneNumber = "invalid"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_WithTakenPhoneNumber_ShouldReturnBadRequest()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = _generate.NewUser(
            username: "testusertest3",
            firstName: "Test",
            lastName: "User",
            passwordHash: hashedPassword.Hash,
            passwordSalt: hashedPassword.Salt,
            birthDate: new DateTime(1990, 1, 1),
            gender: UserGender.Male,
            state: UserState.Active
        );
        await userRepo.AddUserAsync(existingUser);

        var existingPhone = _generate.NewPhoneNumber(
            value: "+1234567890",
            userId: existingUser.Id,
            type: PhoneType.Primary,
            state: PhoneState.Active,
            user: existingUser
        );
        await userRepo.AddPhoneNumberAsync(existingPhone);

        var request = new RegisterRequest
        {
            Username = "testusertest4",
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test4.com",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            Password = "Password123!",
            PhoneNumber = "+1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_WithVeryOldBirthDate_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "testusertest6",
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test6.com",
            BirthDate = new DateTime(1899, 1, 1),
            Gender = UserGender.Male,
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_WithInvalidCharactersInName_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "testusertest7",
            FirstName = "Test123",
            LastName = "User!@#",
            Email = "testuser@test7.com",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}