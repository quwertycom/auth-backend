using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;

namespace API.Features.Authentication.EmailVerification.Endpoints;

public class VerifyEmailEndpoint : Endpoint<VerifyEmailRequest, VerifyEmailResponse> {
  private readonly IEmailVerificationService _emailVerificationService;

  public VerifyEmailEndpoint(IEmailVerificationService emailVerificationService) {
    _emailVerificationService = emailVerificationService;
  }

  public override void Configure() {
    Post("/api/authentication/email-verification/verify");
    AllowAnonymous();
  }

  public override async Task HandleAsync(VerifyEmailRequest req, CancellationToken ct) {
    var result = await _emailVerificationService.VerifyEmailAsync(req.EmailVerificationSessionId, req.Code, ct);

    if (result.IsSuccess) {
      await SendAsync(new VerifyEmailResponse {
        Status = result.Status,
        Message = result.Message ?? "Email verified successfully"
      }, statusCode: 200, ct);
    } else {
      int statusCode = result.Status switch {
        "SESSION_NOT_FOUND" => 404,
        "SESSION_EXPIRED" => 410,
        "INVALID_CODE" => 400,
        "ERROR" => 500,
        _ => 400
      };

      await SendAsync(new VerifyEmailResponse {
        Status = result.Status,
        Message = result.Message
      }, statusCode, ct);
    }
  }
}
