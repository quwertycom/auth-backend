using API.Shared.Contracts.Responses.Common;

namespace API.Features.Authentication.Register.Models;

public record RegisterResponse : ResponseBase
{
    public required string EmailVerificationSessionId { get; set; }
}
