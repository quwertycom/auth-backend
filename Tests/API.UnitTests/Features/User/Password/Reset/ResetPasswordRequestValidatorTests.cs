using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Validation;
using FluentValidation.TestHelper;

namespace API.Tests.Features.User.Password.Reset.Validation;

public class ResetPasswordRequestValidatorTests
{
    private ResetPasswordRequestValidator? _validator;

    #region Setup
    [SetUp]
    public void Setup()
    {
        _validator = new ResetPasswordRequestValidator();
    }
    #endregion

    #region Code Validation
    [Test]
    public void Validate_WhenCodeIsCorrectLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = "ValidPassword1" }; // Valid code

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Test]
    public void Validate_WhenCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = string.Empty, NewPassword = "ValidPassword1" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Code is required!");
    }

    [Test]
    public void Validate_WhenCodeIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = "1234", NewPassword = "ValidPassword1" }; // Shorter than 64 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Code must be 64 characters long!");
    }

    [Test]
    public void Validate_WhenCodeIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 65), NewPassword = "ValidPassword1" }; // Longer than 64 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Code must be 64 characters long!");
    }
    #endregion

    #region NewPassword Validation
    [Test]
    public void Validate_WhenNewPasswordIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = "ValidPassword1" }; // Valid password

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Test]
    public void Validate_WhenNewPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = string.Empty };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("New password is required!");
    }

    [Test]
    public void Validate_WhenNewPasswordIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = "Short1" }; // Shorter than 8 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("New password must be at least 8 characters long!");
    }

    [Test]
    public void Validate_WhenNewPasswordIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = new string('A', 33) }; // Longer than 32 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("New password must be less than 32 characters long!");
    }

    [Test]
    public void Validate_WhenNewPasswordDoesNotContainLowercase_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = "UPPERCASE1" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("New password must contain at least one lowercase letter!");
    }

    [Test]
    public void Validate_WhenNewPasswordDoesNotContainUppercase_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = "lowercase1" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("New password must contain at least one uppercase letter!");
    }

    [Test]
    public void Validate_WhenNewPasswordDoesNotContainNumber_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest { Code = new string('A', 64), NewPassword = "UpperCasePassword" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("New password must contain at least one number!");
    }
    #endregion
}