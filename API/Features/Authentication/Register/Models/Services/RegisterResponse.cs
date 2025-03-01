using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.Register.Models.Services;

public record RegisterResponse: ServiceResult
{
    public required string EmailVerificationSessionId { get; set; }
}
