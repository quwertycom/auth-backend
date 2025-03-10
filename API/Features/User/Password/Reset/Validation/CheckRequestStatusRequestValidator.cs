using FastEndpoints;
using FluentValidation;
using API.Features.User.Password.Reset.Models.Contracts;

namespace API.Features.User.Password.Reset.Validation;

public class CheckRequestStatusRequestValidator : Validator<CheckRequestStatusRequest>
{
    public CheckRequestStatusRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required!")
            .Length(64)
            .WithMessage("Code must be 64 characters long!");
    }
}