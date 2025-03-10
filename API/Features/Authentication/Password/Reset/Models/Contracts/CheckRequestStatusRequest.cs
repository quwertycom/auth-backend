using FastEndpoints;

namespace API.Features.Authentication.Password.Reset.Models.Contracts;

public record CheckRequestStatusRequest
{
    public required string Code { get; set; }
}