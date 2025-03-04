using API.Features.Authentication.Register.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Authentication.Register.Validation;

public class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
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
            .MinimumLength(10)
            .WithMessage("Phone number must be at least 10 characters long!")
            .MaximumLength(20)
            .WithMessage("Phone number must be less than 20 characters long!")
            .Matches(@"^\+([0-9]{1,3})?[0-9]+$")
            .WithMessage("Phone number must start with '+' and contain only numbers and optional region code (e.g., +123)");

        RuleFor(x => x.BirthDate)
            .NotNull()
            .WithMessage("Birth date is required!")
            .Must(birthDate => birthDate <= DateTime.Now.AddYears(-16))
            .WithMessage("You must be at least 16 years old!")
            .Must(birthDate => birthDate >= new DateTime(1900, 1, 1))
            .WithMessage("Birth date cannot be older than 1900 year!");

        RuleFor(x => x.Gender)
            .NotNull()
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