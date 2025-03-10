using FastEndpoints;
using FluentValidation;
using API.Features.User.Password.Reset.Models.Contracts;

namespace API.Features.User.Password.Reset.Validation;

public class ResetPasswordRequestValidator : Validator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required!")
            .Length(64)
            .WithMessage("Code must be 64 characters long!");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required!")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long!")
            .MaximumLength(32)
            .WithMessage("New password must be less than 32 characters long!")
            .Matches(@"[a-z]")
            .WithMessage("New password must contain at least one lowercase letter!")
            .Matches(@"[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter!")
            .Matches(@"[0-9]")
            .WithMessage("New password must contain at least one number!");
    }
}