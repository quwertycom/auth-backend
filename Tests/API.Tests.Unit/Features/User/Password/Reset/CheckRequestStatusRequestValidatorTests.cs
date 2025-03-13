using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Validation;
using FluentValidation.TestHelper;

namespace API.Tests.Unit.Features.User.Password.Reset;

public class CheckRequestStatusRequestValidatorTests
{
    private CheckRequestStatusRequestValidator? _validator;

    #region Setup
    [SetUp]
    public void Setup()
    {
        _validator = new CheckRequestStatusRequestValidator();
    }
    #endregion

    #region Code Validation
    [Test]
    public void Validate_WhenCodeIsCorrectLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CheckRequestStatusRequest { Code = new string('A', 64) }; // Exactly 64 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Test]
    public void Validate_WhenCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CheckRequestStatusRequest { Code = string.Empty };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Code is required!");
    }

    [Test]
    public void Validate_WhenCodeIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CheckRequestStatusRequest { Code = "1234" }; // Shorter than 64 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Code must be 64 characters long!");
    }

    [Test]
    public void Validate_WhenCodeIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CheckRequestStatusRequest { Code = new string('A', 65) }; // Longer than 64 chars

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Code must be 64 characters long!");
    }
    #endregion
}