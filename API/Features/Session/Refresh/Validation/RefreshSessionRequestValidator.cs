using API.Features.Session.Refresh.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Session.Refresh.Validation;

public class RefreshSessionRequestValidator : Validator<RefreshSessionRequest>
{
    public RefreshSessionRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required!");
    }
}