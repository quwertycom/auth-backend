using NUnit.Framework;
using API.Features.Authentication.Register.Endpoints;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using API.Features.Authentication.Register.Models.Contracts;
using FluentAssertions;
using NSubstitute;
using API.Features.Authentication.Register.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using API.Features.Authentication.Register.Models.Services;
using API.Features.Authentication.Register.Validation;
using FluentValidation.Results;

namespace API.UnitTests.Features.Authentication.Register;

public class RegisterEndpointTests : TestBase
{
    [Test]
    public async Task RegisterEndpoint_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };

        MockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = true, Status = "SUCCESS", EmailVerificationSessionId = "1234567890" });

        var endpoint = new RegisterEndpoint(MockRegisterService);

        // Act
        var result = await endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<Ok<RegisterResponse>>();
        if (result.Result == null)
        {
            NUnit.Framework.Assert.Fail("Expected Ok<RegisterResponse> result but got null.");
            return; // Early exit to prevent further null-related errors
        }

        if (!(result.Result is Ok<RegisterResponse> okResult))
        {
            NUnit.Framework.Assert.Fail($"Expected Ok<RegisterResponse> but got {result.Result.GetType().Name}");
            return; // Early exit if the type is incorrect
        }

        okResult.Value.Should().NotBeNull();
        okResult.Value?.Status.Should().Be("SUCCESS");
        okResult.Value?.EmailVerificationSessionId.Should().NotBeNull();
        okResult.Value?.EmailVerificationSessionId.Should().NotBeEmpty();
        okResult.Value?.EmailVerificationSessionId.Should().Be("1234567890");

        await MockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.Username == request.Username &&
                               r.Email == request.Email &&
                               r.Password == request.Password),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_UsernameExists_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existinguser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };

        MockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "USERNAME_EXISTS", Message = "Username already exists" });

        var endpoint = new RegisterEndpoint(MockRegisterService);

        // Act
        var result = await endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse>>();
        if (result.Result == null)
        {
            NUnit.Framework.Assert.Fail("Expected BadRequest<ErrorResponse> result but got null.");
            return; // Early exit
        }

        if (!(result.Result is BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse> badRequestResult))
        {
            NUnit.Framework.Assert.Fail($"Expected BadRequest<ErrorResponse> but got {result.Result?.GetType().Name}");
            return; // Early exit if the type is incorrect
        }

        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value?.Details.Should().NotBeNull();
        badRequestResult.Value?.Status.Should().Be("USERNAME_EXISTS");
        badRequestResult.Value?.Message.Should().Be("Username already exists");
        badRequestResult.Value?.Details.Should().ContainKey("username");
        badRequestResult.Value?.Details?["username"].Should().Contain("Username already exists");

        await MockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.Username == request.Username),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_EmailExists_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "existing@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };

        MockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "EMAIL_EXISTS", Message = "Email already exists" });

        var endpoint = new RegisterEndpoint(MockRegisterService);

        // Act
        var result = await endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse>>();
        if (result.Result == null)
        {
            NUnit.Framework.Assert.Fail("Expected BadRequest<ErrorResponse> result but got null.");
            return; // Early exit
        }

        if (!(result.Result is BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse> badRequestResult))
        {
            NUnit.Framework.Assert.Fail($"Expected BadRequest<ErrorResponse> but got {result.Result?.GetType().Name}");
            return; // Early exit if the type is incorrect
        }

        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value?.Details.Should().NotBeNull();
        badRequestResult.Value?.Status.Should().Be("EMAIL_EXISTS");
        badRequestResult.Value?.Message.Should().Be("Email already exists");
        badRequestResult.Value?.Details.Should().ContainKey("email");
        badRequestResult.Value?.Details?["email"].Should().Contain("Email already exists");

        await MockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.Email == request.Email),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_PhoneNumberExists_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            PhoneNumber = "+1234567890",
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };

        MockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "PHONE_NUMBER_EXISTS", Message = "Phone number already exists" });

        var endpoint = new RegisterEndpoint(MockRegisterService);

        // Act
        var result = await endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse>>();
        if (result.Result == null)
        {
            NUnit.Framework.Assert.Fail("Expected BadRequest<ErrorResponse> result but got null.");
            return; // Early exit
        }

        if (!(result.Result is BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse> badRequestResult))
        {
            NUnit.Framework.Assert.Fail($"Expected BadRequest<ErrorResponse> but got {result.Result?.GetType().Name}");
            return; // Early exit if the type is incorrect
        }

        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value?.Details.Should().NotBeNull();
        badRequestResult.Value?.Status.Should().Be("PHONE_NUMBER_EXISTS");
        badRequestResult.Value?.Message.Should().Be("Phone number already exists");
        badRequestResult.Value?.Details.Should().ContainKey("phoneNumber");
        badRequestResult.Value?.Details?["phoneNumber"].Should().Contain("Phone number already exists");

        await MockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.PhoneNumber == request.PhoneNumber),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_InternalServerError_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };

        MockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later." });

        var endpoint = new RegisterEndpoint(MockRegisterService);

        // Act
        var result = await endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse>>();
        if (result.Result == null)
        {
            NUnit.Framework.Assert.Fail("Expected BadRequest<ErrorResponse> result but got null.");
            return; // Early exit
        }

        if (!(result.Result is BadRequest<API.Shared.Contracts.Responses.Common.ErrorResponse> badRequestResult))
        {
            NUnit.Framework.Assert.Fail($"Expected BadRequest<ErrorResponse> but got {result.Result?.GetType().Name}");
            return; // Early exit if the type is incorrect
        }

        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value?.Status.Should().Be("INTERNAL_SERVER_ERROR");
        badRequestResult.Value?.Message.Should().Be("Something went wrong, please try again later.");
        badRequestResult.Value?.Details.Should().BeEmpty();

        await MockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>());
    }
}