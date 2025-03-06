using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Features.Authentication.EmailVerification.Validation;
using FluentValidation.TestHelper;

namespace API.UnitTests.Features.Authentication.EmailVerification;

public class RequestNewCodeRequestValidatorTests : TestBase
{
    #region Helper Methods
    
    private RequestNewCodeRequestValidator CreateValidator()
    {
        return new RequestNewCodeRequestValidator();
    }
    
    private RequestNewCodeRequest CreateRequest(string email = "test@example.com")
    {
        return new RequestNewCodeRequest
        {
            Email = email
        };
    }
    
    #endregion
    
    #region Email Validation Tests
    
    [Test]
    public async Task Validate_ValidEmail_PassesValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest("valid@example.com");
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Test]
    public async Task Validate_EmptyEmail_FailsValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest("");
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required!");
    }
    
    [Test]
    public async Task Validate_NullEmail_FailsValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest(null!);
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required!");
    }
    
    [Test]
    public async Task Validate_InvalidEmailFormat_FailsValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest("invalid-email");
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is invalid!");
    }
    
    [Test]
    public async Task Validate_EmailWithoutAtSymbol_FailsValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest("invalidemail.com");
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is invalid!");
    }
    
    [Test]
    public async Task Validate_EmailWithoutDomain_FailsValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest("user@");
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is invalid!");
    }
    
    [Test]
    public async Task Validate_EmailWithMultipleAtSymbols_FailsValidation()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateRequest("user@domain@example.com");
        
        // Act
        var result = await validator.TestValidateAsync(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is invalid!");
    }
    
    #endregion
}