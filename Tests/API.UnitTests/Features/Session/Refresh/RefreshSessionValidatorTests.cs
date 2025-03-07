using API.Features.Session.Refresh.Models.Contracts;
using API.Features.Session.Refresh.Validation;
using FluentValidation.TestHelper;

namespace API.UnitTests.Features.Session.Refresh;

public class RefreshSessionValidatorTests : TestBase
{
    private RefreshSessionRequestValidator? _validator;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _validator = new RefreshSessionRequestValidator();
    }

    [Test]
    public void Validate_WhenTokenIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RefreshSessionRequest { Token = string.Empty };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token is required!");
    }

    [Test]
    public void Validate_WhenTokenIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RefreshSessionRequest { Token = null! };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Test]
    public void Validate_WhenTokenIsProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new RefreshSessionRequest { Token = "valid-refresh-token" };

        // Act & Assert
        var result = _validator!.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}