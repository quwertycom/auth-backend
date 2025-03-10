// Unit test file
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Validation;
using FluentValidation.TestHelper;

namespace API.Tests.Features.User.Password.Reset;

public class RequestPasswordResetRequestValidatorTests
{
    private RequestPasswordResetRequestValidator? _validator;

    #region Setup
    [SetUp]
    public void Setup()
    {
        _validator = new RequestPasswordResetRequestValidator();
    }
    #endregion

    #region Email Validation
    [Test]
    public void Validate_WhenEmailIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = "test@example.com" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = string.Empty };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required!");
    }

    [Test]
    public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = "invalid-email" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email address!");
    }
    #endregion
}