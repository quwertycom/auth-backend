using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.Register.Models.Services;

public record RegisterResponse: ServiceResult
{
    public string? EmailVerificationSessionId { get; set; }
}
