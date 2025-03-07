using API.Features.Session.Revoke.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Session.Revoke.Validation;

public class RevokeSessionRequestValidator : Validator<RevokeSessionRequest>
{
    public RevokeSessionRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required!")
            .Matches(@"^[1-9]\d*$")
            .WithMessage("Session ID must be a positive number!");
    }
}