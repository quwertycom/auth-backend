using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Features.Authentication.EmailVerification.Validation;
using FluentValidation.TestHelper;

namespace API.UnitTests.Features.Authentication.EmailVerification;

public class VerifyEmailRequestValidatorTests : TestBase
{
    #region Helper Methods

    private VerifyEmailRequest CreateValidRequest()
    {
        return new VerifyEmailRequest
        {
            RequestId = "123456",
            Code = "123456"
        };
    }

    #endregion

    #region RequestId Validation Tests

    [Test]
    public void Validate_WhenRequestIdIsValid_ShouldNotHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.RequestId);
    }

    [Test]
    public void Validate_WhenRequestIdIsEmpty_ShouldHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestId = string.Empty;
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId is required!");
    }

    [Test]
    public void Validate_WhenRequestIdIsNull_ShouldHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestId = null!;
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId is required!");
    }

    [Test]
    public void Validate_WhenRequestIdIsNotNumeric_ShouldHaveErrorForRequestId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestId = "abc123";
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId must be a number!");
    }

    #endregion

    #region Code Validation Tests

    [Test]
    public void Validate_WhenCodeIsValid_ShouldNotHaveErrorForCode()
    {
        // Arrange
        var request = CreateValidRequest();
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Test]
    public void Validate_WhenCodeIsEmpty_ShouldHaveErrorForCode()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = string.Empty;
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code is required!");
    }

    [Test]
    public void Validate_WhenCodeIsNull_ShouldHaveErrorForCode()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = null!;
        var validator = new VerifyEmailRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code is required!");
    }

    #endregion
}