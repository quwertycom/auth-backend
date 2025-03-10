using FastEndpoints;
using FluentValidation;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.Authentication.Register.Validation;

namespace API.Features.User.Password.Reset.Validation;

public class RequestPasswordResetRequestValidator : Validator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => new { x.Email, x.Username })
            .Must(x => !string.IsNullOrEmpty(x.Email) ^ !string.IsNullOrEmpty(x.Username))
            .WithMessage("Either Email or Username must be specified, but not both.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email address!")
            .When(x => string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required!")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long!")
            .MaximumLength(32)
            .WithMessage("Username must be less than 32 characters long!")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username must contain only letters, numbers, and underscores!")
            .When(x => string.IsNullOrEmpty(x.Email));
    }
}