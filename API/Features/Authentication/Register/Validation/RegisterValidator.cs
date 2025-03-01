using API.Features.Authentication.Register.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Authentication.Register.Validation;

public class RegisterValidator : Validator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required!")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long!")
            .MaximumLength(32)
            .WithMessage("Username must be less than 32 characters long!")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username must contain only letters, numbers, and underscores!");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required!")
            .MaximumLength(128)
            .WithMessage("First name must be less than 128 characters long!")
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("First name must contain only letters and spaces!");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required!")
            .MaximumLength(128)
            .WithMessage("Last name must be less than 128 characters long!")
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("Last name must contain only letters and spaces!");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .EmailAddress()
            .WithMessage("Email is invalid!")
            .MaximumLength(256)
            .WithMessage("Email must be less than 256 characters long!");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required!")
            .MinimumLength(10)
            .WithMessage("Phone number must be at least 10 characters long!")
            .MaximumLength(16)
            .WithMessage("Phone number must be less than 16 characters long!") // Updated to 16 to match PhoneNumber.Value max length
            .Matches(@"^[0-9]+$")
            .WithMessage("Phone number must contain only numbers!");

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Birth date is required!");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("Gender is required!");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required!")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long!")
            .MaximumLength(32)
            .WithMessage("Password must be less than 32 characters long!")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter!")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter!")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number!");
    }
}