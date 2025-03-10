using FastEndpoints;

namespace API.Features.User.Password.Reset.Models.Contracts;

public record CheckRequestStatusRequest
{
    public required string Code { get; set; }
}