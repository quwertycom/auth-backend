using Microsoft.AspNetCore.Mvc;

namespace API.Features.Authentication.EmailVerification.Models.Contracts;

public record RequestStatusRequest
{
    [FromQuery]
    public required string RequestId { get; set; }

    [FromQuery]
    public required string Email { get; set; }
}