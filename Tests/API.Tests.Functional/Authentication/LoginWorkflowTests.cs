using System.Net;
using System.Net.Http.Json;
using API.Features.Authentication.Login.Models.Contracts;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using FluentAssertions;
using NUnit.Framework;

namespace API.Tests.Functional.Authentication;

[TestFixture]
public class LoginWorkflowTests : TestBase
{
    [Test]
    public async Task Login_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/authentication/login");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task Login_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = new User
        {
            Username = "testusertest1",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        await userRepo.AddUserAsync(existingUser);

        var request = new LoginRequest
        {
            Username = "testusertest1",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = new User
        {
            Username = "testusertest2",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        await userRepo.AddUserAsync(existingUser);

        var request = new LoginRequest
        {
            Username = "testusertest2",
            Password = "InvalidPassword"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithSuspendedUser_ShouldReturnUnauthorized()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = new User
        {
            Username = "testusertest3",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.Suspended
        };
        await userRepo.AddUserAsync(existingUser);

        var request = new LoginRequest
        {
            Username = "testusertest3",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithDeletedUser_ShouldReturnUnauthorized()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = new User
        {
            Username = "testusertest4",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.Deleted
        };
        await userRepo.AddUserAsync(existingUser);

        var request = new LoginRequest
        {
            Username = "testusertest4",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithIncorrectUsername_ShouldReturnUnauthorized()
    {
        var userRepo = GetRequiredService<IUserRepository>();
        var hasher = GetRequiredService<IHasher>();

        var hashedPassword = hasher.Hash("Password123!");
        var existingUser = new User
        {
            Username = "testusertest5",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt,
            BirthDate = new DateTime(1990, 1, 1),
            Gender = UserGender.Male,
            State = UserState.Active
        };
        await userRepo.AddUserAsync(existingUser);

        var request = new LoginRequest
        {
            Username = "incorrectUsername",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/authentication/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
