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
        var okResult = result.Result as Ok<RegisterResponse>;
        okResult?.Value?.Status.Should().Be("SUCCESS");
        okResult?.Value?.EmailVerificationSessionId.Should().NotBeNullOrEmpty();
        okResult?.Value?.EmailVerificationSessionId.Should().Be("1234567890");

        await MockRegisterService
            .Received(1)
            .RegisterUserAsync(Arg.Is<RegisterRequest>(r => 
                               r.Username == request.Username &&
                               r.Email == request.Email &&
                               r.Password == request.Password), 
                               Arg.Any<CancellationToken>());
    }


    [Test]
    public async Task RegisterValidator_EmptyRequest_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterValidator();
        var request = new RegisterRequest
        {
            Username = "",
            FirstName = "",
            LastName = "",
            Email = "",
            Password = "",
            BirthDate = DateTime.MinValue,
            Gender = API.Shared.Enums.Entities.User.UserGender.Male
        };

        // Act
        var validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(17);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "BirthDate");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Gender");
    }
}
