using API.Features.Authentication.Login.Models.Contracts;
using FastEndpoints;
using FluentValidation;

namespace API.Features.Authentication.Login.Validation;

public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required!")
            .Matches(@"^[a-zA-Z0-9]+$")
            .WithMessage("Username must contain only letters and numbers!");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required!");
    }
}