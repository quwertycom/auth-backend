using Microsoft.AspNetCore.Http.HttpResults;
using API.Features.Authentication.Register.Endpoints;
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.Register.Models.Services;
using API.Features.Authentication.Register.Interfaces;

namespace API.UnitTests.Features.Authentication.Register;

public class RegisterEndpointTests : TestBase
{
    #region Helper Methods

    private RegisterRequest CreateDefaultRegisterRequest(
        string username = "testuser",
        string firstName = "Test",
        string lastName = "User",
        string email = "test@example.com",
        string password = "Password123!",
        string? phoneNumber = null)
    {
        return new RegisterRequest
        {
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            BirthDate = DateTime.Now.AddYears(-20),
            PhoneNumber = phoneNumber,
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };
    }

    private RegisterEndpoint CreateEndpoint(IRegisterService registerService)
    {
        return new RegisterEndpoint(registerService);
    }

    #endregion

    #region RegisterEndpoint Tests

    [Test]
    public async Task RegisterEndpoint_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        var mockRegisterService = Substitute.For<IRegisterService>();

        mockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = true, Status = "SUCCESS", RequestId = "1234567890" });

        var endpoint = CreateEndpoint(mockRegisterService);

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
        okResult.Value?.RequestId.Should().NotBeNull();
        okResult.Value?.RequestId.Should().NotBeEmpty();
        okResult.Value?.RequestId.Should().Be("1234567890");

        await mockRegisterService
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
        var request = CreateDefaultRegisterRequest(username: "existinguser");
        var mockRegisterService = Substitute.For<IRegisterService>();

        mockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "USERNAME_EXISTS", Message = "Username already exists" });

        var endpoint = CreateEndpoint(mockRegisterService);

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

        await mockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.Username == request.Username),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_EmailExists_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(email: "existing@example.com");
        var mockRegisterService = Substitute.For<IRegisterService>();

        mockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "EMAIL_EXISTS", Message = "Email already exists" });

        var endpoint = CreateEndpoint(mockRegisterService);

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

        await mockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.Email == request.Email),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_PhoneNumberExists_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest(phoneNumber: "+1234567890");
        var mockRegisterService = Substitute.For<IRegisterService>();

        mockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "PHONE_NUMBER_EXISTS", Message = "Phone number already exists" });

        var endpoint = CreateEndpoint(mockRegisterService);

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

        await mockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r =>
                               r.PhoneNumber == request.PhoneNumber),
                               Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterEndpoint_InternalServerError_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateDefaultRegisterRequest();
        var mockRegisterService = Substitute.For<IRegisterService>();

        mockRegisterService.RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
           .Returns(new RegisterResult { IsSuccess = false, Status = "INTERNAL_SERVER_ERROR", Message = "Something went wrong, please try again later." });

        var endpoint = CreateEndpoint(mockRegisterService);

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

        await mockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>());
    }

    #endregion
}