using API.Features.Session.Revoke.Models.Contracts;
using API.Features.Session.Revoke.Validation;
using FluentValidation.TestHelper;

namespace API.UnitTests.Features.Session.Revoke;

public class RevokeSessionRequestValidatorTests : TestBase
{
    #region Helper Methods

    private RevokeSessionRequest CreateValidRequest()
    {
        return new RevokeSessionRequest
        {
            SessionId = "123"
        };
    }

    #endregion

    #region SessionId Validation Tests

    [Test]
    public void Validate_WhenSessionIdIsValid_ShouldNotHaveErrorForSessionId()
    {
        // Arrange
        var request = CreateValidRequest();
        var validator = new RevokeSessionRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SessionId);
    }

    [Test]
    public void Validate_WhenSessionIdIsEmpty_ShouldHaveErrorForSessionId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SessionId = string.Empty;
        var validator = new RevokeSessionRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage("Session ID is required!");
    }

    [Test]
    public void Validate_WhenSessionIdIsNull_ShouldHaveErrorForSessionId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SessionId = null!;
        var validator = new RevokeSessionRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage("Session ID is required!");
    }

    [Test]
    public void Validate_WhenSessionIdIsNotNumeric_ShouldHaveErrorForSessionId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SessionId = "abc123";
        var validator = new RevokeSessionRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage("Session ID must be a positive number!");
    }

    [Test]
    public void Validate_WhenSessionIdIsZero_ShouldHaveErrorForSessionId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SessionId = "0";
        var validator = new RevokeSessionRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage("Session ID must be a positive number!");
    }

    [Test]
    public void Validate_WhenSessionIdIsNegative_ShouldHaveErrorForSessionId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SessionId = "-1";
        var validator = new RevokeSessionRequestValidator();

        // Act & Assert
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage("Session ID must be a positive number!");
    }

    #endregion
} 