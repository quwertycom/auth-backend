using API.Features.Authentication.Login.Models.Contracts;
using API.Features.Authentication.Login.Validation;
using FluentValidation.TestHelper;

namespace API.UnitTests.Features.Authentication.Login;

public class LoginRequestValidatorTests : TestBase
{
    private LoginRequestValidator? _validator;

    #region Setup

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _validator = new LoginRequestValidator();
    }

    #endregion

    #region Helper Methods

    private LoginRequest CreateValidRequest()
    {
        return new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };
    }

    #endregion

    #region Username Validation Tests

    [Test]
    public void Validate_WhenUsernameIsValid_ShouldNotHaveErrorForUsername()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveErrorForUsername()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = string.Empty;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required!");
    }

    [Test]
    public void Validate_WhenUsernameIsNull_ShouldHaveErrorForUsername()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = null!;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required!");
    }

    [Test]
    public void Validate_WhenUsernameContainsInvalidCharacters_ShouldHaveErrorForUsername()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = "test-user@123";

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must contain only letters and numbers!");
    }

    [Test]
    public void Validate_WhenUsernameContainsOnlyLettersAndNumbers_ShouldNotHaveErrorForUsername()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = "testUser123";

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    #endregion

    #region Password Validation Tests

    [Test]
    public void Validate_WhenPasswordIsValid_ShouldNotHaveErrorForPassword()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = string.Empty;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required!");
    }

    [Test]
    public void Validate_WhenPasswordIsNull_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = null!;

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required!");
    }

    #endregion
}