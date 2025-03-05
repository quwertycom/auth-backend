using API.Features.Authentication.Register.Validation;
using API.Features.Authentication.Register.Models.Contracts;
using API.Shared.Enums.Entities.User;

namespace API.UnitTests.Features.Authentication.Register;

public class RegisterRequestValidatorTests : TestBase
{
    [Test]
    public async Task RegisterRequestValidator_EmptyRequest_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "",
            FirstName = "",
            LastName = "",
            Email = "",
            Password = "",
            BirthDate = DateTime.Now,
            Gender = UserGender.Male
        };

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

    [Test]
    public async Task RegisterRequestValidator_EmptyEmail_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = (UserGender)0
        };

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Email" && error.ErrorMessage == "Email is invalid!");
    }

    [Test]
    public async Task RegisterRequestValidator_TooShortPhoneNumber_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            PhoneNumber = "+12345678",
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            PhoneNumber = "+12345678901234567890",
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            PhoneNumber = "1234567890",
            Gender = UserGender.Male
        };

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "PhoneNumber" && error.ErrorMessage == "Phone number must start with '+' and contain only numbers and optional region code (e.g., +123)");
    }

    [Test]
    public async Task RegisterRequestValidator_EmptyPassword_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Short1",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "password123",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "PASSWORD123",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "1234567890",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(2);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one lowercase letter!");
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Password" && error.ErrorMessage == "Password must contain at least one uppercase letter!");
    }

    [Test]
    public async Task RegisterRequestValidator_EmptyUsername_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "invalid-username",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "ab",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "thisisareallylongusernamethatisover32characters",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "Username" && error.ErrorMessage == "Username must be less than 32 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_EmptyFirstName_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "1234567890",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "ThisIsAReallyLongFirstNameThatIsLongerThanOneHoundretTwentyEightCharactersSoItNeedToBeReallyLongToAchieveTheMaximumLengthLoremIpsumDolorSitAmetConsecteturAdipiscingElitSedDoEiusmodTemporIncididuntUtLaboreEtDoloreMagnaAliquaUtEnimAdMinimVeniamQuisNostrudExercitationUllamcoLaborisNisiUtAliquipExEaCommodoConsequatDuisAuteIrureDolorInReprehenderitInVoluptateVelitEsseCillumDolo",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "FirstName" && error.ErrorMessage == "First name must be less than 128 characters long!");
    }

    [Test]
    public async Task RegisterRequestValidator_EmptyLastName_HasValidationErrors()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "1234567890",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

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
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "ThisIsAReallyLongLastNameThatIsLongerThanOneHoundretTwentyEightCharactersSoItNeedToBeReallyLongToAchieveTheMaximumLengthLoremIpsumDolorSitAmetConsecteturAdipiscingElitSedDoEiusmodTemporIncididuntUtLaboreEtDoloreMagnaAliquaUtEnimAdMinimVeniamQuisNostrudExercitationUllamcoLaborisNisiUtAliquipExEaCommodoConsequatDuisAuteIrureDolorInReprehenderitInVoluptateVelitEsseCillumDolo",
            Email = "test@example.com",
            Password = "Password123!",
            BirthDate = DateTime.Now.AddYears(-20),
            Gender = UserGender.Male
        };

        // Act
        ValidationResult validationResult = await validator.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(error => error.PropertyName == "LastName" && error.ErrorMessage == "Last name must be less than 128 characters long!");
    }
}
