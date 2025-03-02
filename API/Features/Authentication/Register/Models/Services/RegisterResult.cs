using API.Shared.Models.Features.Services;

namespace API.Features.Authentication.Register.Models.Services;

public record RegisterResult : ServiceResult
{
    public string? EmailVerificationSessionId { get; set; }
}
