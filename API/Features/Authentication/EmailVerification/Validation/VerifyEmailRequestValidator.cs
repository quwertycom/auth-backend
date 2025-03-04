using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;
using FluentValidation;

public class VerifyEmailRequestValidator : Validator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("RequestId is required!")
            .Matches(@"^[0-9]+$")
            .WithMessage("RequestId must be a number!");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required!");
    }
}
