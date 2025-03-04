using API.Shared.Contracts.Responses.Common;

namespace API.Features.Authentication.Register.Models.Contracts;

public record RegisterResponse : ResponseBase
{
    public required string? RequestId { get; set; }
}
