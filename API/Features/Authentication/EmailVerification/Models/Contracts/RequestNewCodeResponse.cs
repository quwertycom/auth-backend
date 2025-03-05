
using API.Shared.Contracts.Responses.Common;

namespace API.Features.Authentication.EmailVerification.Models.Contracts;

public record RequestNewCodeResponse : ResponseBase
{
    public required string? NewRequestId { get; set; }
}
