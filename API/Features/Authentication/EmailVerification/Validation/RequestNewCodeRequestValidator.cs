using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Authentication.EmailVerification.Validation;

public class RequestNewCodeRequestValidator : Validator<RequestNewCodeRequest>
{
    public RequestNewCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .EmailAddress()
            .WithMessage("Email is invalid!");
    }
}

