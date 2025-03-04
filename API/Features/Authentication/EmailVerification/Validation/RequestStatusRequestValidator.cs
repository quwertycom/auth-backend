using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Authentication.EmailVerification.Validation;

public class RequestStatusRequestValidator : Validator<RequestStatusRequest>
{
    public RequestStatusRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("RequestId is required!")
            .Matches(@"^[0-9]+$")
            .WithMessage("RequestId must be a number!");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .EmailAddress()
            .WithMessage("Email is invalid!");
    }
}

