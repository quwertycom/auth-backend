using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Validation;
using FluentValidation.TestHelper;

namespace API.Tests.Unit.Features.User.Password.Reset;

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
    public void Validate_WhenEmailIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = string.Empty, Username = "username" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
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

    #region Username Validation
    [Test]
    public void Validate_WhenUsernameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Username = "test_user" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Username = string.Empty, Email = null };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is required!");
    }

    [Test]
    public void Validate_WhenUsernameIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Username = "ab" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must be at least 3 characters long!");
    }

    [Test]
    public void Validate_WhenUsernameIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Username = "this_username_is_too_long_username" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must be less than 32 characters long!");
    }

    [Test]
    public void Validate_WhenUsernameIsInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Username = "invalid-username!" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must contain only letters, numbers, and underscores!");
    }
    #endregion

    #region Email or Username Validation
    [Test]
    public void Validate_WhenNeitherEmailNorUsernameIsProvided_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = null, Username = null };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(string.Empty)
              .WithErrorMessage("Either Email or Username must be specified, but not both.");
    }

    [Test]
    public void Validate_WhenBothEmailAndUsernameAreProvided_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = "test@example.com", Username = "testuser" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(string.Empty)
              .WithErrorMessage("Either Email or Username must be specified, but not both.");
    }

    [Test]
    public void Validate_WhenOnlyEmailIsProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = "test@example.com", Username = null };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WhenOnlyUsernameIsProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new RequestPasswordResetRequest { Email = null, Username = "test_user" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
    #endregion
}
