using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;
using FluentValidation;

public class VerifyEmailRequestValidator : Validator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.EmailVerificationSessionId)
            .NotEmpty()
            .WithMessage("EmailVerificationSessionId is required!")
            .Matches(@"^[0-9]+$")
            .WithMessage("EmailVerificationSessionId must be a number!");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required!");
    }
}
