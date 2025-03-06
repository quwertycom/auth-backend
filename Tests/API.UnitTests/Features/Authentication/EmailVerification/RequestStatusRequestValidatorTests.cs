using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Features.Authentication.EmailVerification.Validation;
using FluentValidation.TestHelper;

namespace API.UnitTests.Features.Authentication.EmailVerification;

public class RequestStatusRequestValidatorTests : TestBase
{
    private RequestStatusRequestValidator? _validator;

    #region Setup

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _validator = new RequestStatusRequestValidator();
    }

    #endregion

    #region Helper Methods

    private RequestStatusRequest CreateValidRequest()
    {
        return new RequestStatusRequest
        {
            RequestId = "123456",
            Email = "test@example.com"
        };
    }

    #endregion

    #region RequestId Validation Tests

    [Test]
    public void Validate_WhenRequestIdIsValid_ShouldNotHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.RequestId);
    }

    [Test]
    public void Validate_WhenRequestIdIsEmpty_ShouldHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestId = string.Empty;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId is required!");
    }

    [Test]
    public void Validate_WhenRequestIdIsNull_ShouldHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestId = null!;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId is required!");
    }

    [Test]
    public void Validate_WhenRequestIdIsNotNumeric_ShouldHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestId = "abc123";

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId must be a number!");
    }

    #endregion

    #region Email Validation Tests

    [Test]
    public void Validate_WhenEmailIsValid_ShouldNotHaveErrorForEmail()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WhenEmailIsEmpty_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = string.Empty;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required!");
    }

    [Test]
    public void Validate_WhenEmailIsNull_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = null!;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required!");
    }

    [Test]
    public void Validate_WhenEmailIsInvalid_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = "invalid-email";

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is invalid!");
    }

    #endregion
}