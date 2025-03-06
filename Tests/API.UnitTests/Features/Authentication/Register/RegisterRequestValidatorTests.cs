using API.Features.Authentication.Register.Validation;
using API.Features.Authentication.Register.Models.Contracts;
using API.Shared.Enums.Entities.User;

namespace API.UnitTests.Features.Authentication.Register;

public class RegisterRequestValidatorTests : TestBase
{
    #region Helper Methods

    private RegisterRequestValidator CreateValidator()
    {
        return new RegisterRequestValidator();
    }

    private RegisterRequest CreateDefaultRequest(
        string username = "testuser",
        string firstName = "Test",
        string lastName = "User",
        string email = "test@example.com",
        string password = "Password123!",
        string? phoneNumber = null,
        DateTime? birthDate = null,
        UserGender gender = UserGender.Male)
    {
        return new RegisterRequest
        {
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            PhoneNumber = phoneNumber,
            BirthDate = birthDate ?? DateTime.Now.AddYears(-20),
            Gender = gender
        };
    }

    #endregion

    #region General Validation Tests

    [Test]
    public async Task RegisterRequestValidator_EmptyRequest_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(
            username: "",
            firstName: "",
            lastName: "",
            email: "",
            password: "",
            birthDate: DateTime.UtcNow
        );

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(15);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must contain only letters, numbers, and underscores!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name must contain only letters and spaces!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name must contain only letters and spaces!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email" && error.ErrorMessage == "Email is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email" && error.ErrorMessage == "Email is invalid!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "BirthDate" && error.ErrorMessage == "You must be at least 16 years old!");
    }

    #endregion

    #region Email Validation Tests

    [Test]
    public async Task RegisterRequestValidator_EmptyEmail_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(email: "");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(2);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email" && error.ErrorMessage == "Email is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email" && error.ErrorMessage == "Email is invalid!");
    }

    [Test]
    public async Task RegisterRequestValidator_InvalidEmailFormat_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(email: "invalid-email");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email" && error.ErrorMessage == "Email is invalid!");
    }

    #endregion

    #region Phone Number Validation Tests

    [Test]
    public async Task RegisterRequestValidator_TooShortPhoneNumber_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(phoneNumber: "+12345678");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "PhoneNumber" && error.ErrorMessage == "Phone number must be at least 10 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooLongPhoneNumber_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(phoneNumber: "+12345678901234567890");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "PhoneNumber" && error.ErrorMessage == "Phone number must be less than 20 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_InvalidPhoneNumberFormat_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(phoneNumber: "1234567890");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "PhoneNumber" && error.ErrorMessage == "Phone number must start with '+' and contain only numbers and optional region code (e.g., +123)");
    }

    #endregion

    #region Password Validation Tests

    [Test]
    public async Task RegisterRequestValidator_EmptyPassword_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(password: "");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(5);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must be at least 8 characters long!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one lowercase letter!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one uppercase letter!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one number!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooShortPassword_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(password: "Short1");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must be at least 8 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_AllLowercasePassword_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(password: "password123");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one uppercase letter!");
    }

    [Test]
    public async Task RegisterRequestValidator_AllUppercasePassword_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(password: "PASSWORD123");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one lowercase letter!");
    }

    [Test]
    public async Task RegisterRequestValidator_OnlyNumbersPassword_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(password: "1234567890");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(2);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one lowercase letter!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one uppercase letter!");
    }

    #endregion

    #region Username Validation Tests

    [Test]
    public async Task RegisterRequestValidator_EmptyUsername_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(username: "");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(3);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must contain only letters, numbers, and underscores!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must be at least 3 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_InvalidUsernameFormat_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(username: "invalid-username");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must contain only letters, numbers, and underscores!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooShortUsername_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(username: "ab");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must be at least 3 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooLongUsername_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(username: "thisisareallylongusernamethatisover32characters");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must be less than 32 characters long!");
    }

    #endregion

    #region FirstName Validation Tests

    [Test]
    public async Task RegisterRequestValidator_EmptyFirstName_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(firstName: "");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(2);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name must contain only letters and spaces!");
    }

    [Test]
    public async Task RegisterRequestValidator_OnlyNumbersFirstName_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(firstName: "1234567890");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name must contain only letters and spaces!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooLongFirstName_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(firstName: "ThisIsAReallyLongFirstNameThatIsLongerThanOneHoundretTwentyEightCharactersSoItNeedToBeReallyLongToAchieveTheMaximumLengthLoremIpsumDolorSitAmetConsecteturAdipiscingElitSedDoEiusmodTemporIncididuntUtLaboreEtDoloreMagnaAliquaUtEnimAdMinimVeniamQuisNostrudExercitationUllamcoLaborisNisiUtAliquipExEaCommodoConsequatDuisAuteIrureDolorInReprehenderitInVoluptateVelitEsseCillumDolo");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name must be less than 128 characters long!");
    }

    #endregion

    #region LastName Validation Tests

    [Test]
    public async Task RegisterRequestValidator_EmptyLastName_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(lastName: "");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(2);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name is required!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name must contain only letters and spaces!");
    }

    [Test]
    public async Task RegisterRequestValidator_OnlyNumbersLastName_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(lastName: "1234567890");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name must contain only letters and spaces!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooLongLastName_HasValidationErrors()
    {
        // Arrange
        var validator = CreateValidator();
        var request = CreateDefaultRequest(lastName: "ThisIsAReallyLongLastNameThatIsLongerThanOneHoundretTwentyEightCharactersSoItNeedToBeReallyLongToAchieveTheMaximumLengthLoremIpsumDolorSitAmetConsecteturAdipiscingElitSedDoEiusmodTemporIncididuntUtLaboreEtDoloreMagnaAliquaUtEnimAdMinimVeniamQuisNostrudExercitationUllamcoLaborisNisiUtAliquipExEaCommodoConsequatDuisAuteIrureDolorInReprehenderitInVoluptateVelitEsseCillumDolo");

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name must be less than 128 characters long!");
    }

    #endregion
}
