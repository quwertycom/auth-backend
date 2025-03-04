using Microsoft.AspNetCore.Mvc;

namespace API.Features.Authentication.EmailVerification.Models.Contracts;

public record SessionStatusRequest {
  [FromQuery]
  public required string SessionId { get; set; }

  [FromQuery]
  public required string Email { get; set; }
}