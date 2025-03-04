using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Authentication.EmailVerification.Validation;

public class SessionStatusRequestValidator : Validator<SessionStatusRequest>
{
    public SessionStatusRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required!")
            .Matches(@"^[0-9]+$")
            .WithMessage("SessionId must be a number!");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .EmailAddress()
            .WithMessage("Email is invalid!");
    }
}

