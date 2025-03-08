
using FastEndpoints;
using FluentValidation;
using API.Features.Authentication.Password.Reset.Models.Contracts;
namespace API.Features.Authentication.Password.Reset.Validation;

public class RequestPasswordResetRequestValidator : Validator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .EmailAddress()
            .WithMessage("Invalid email address!");
    }
}