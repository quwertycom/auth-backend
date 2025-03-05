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
    var result = await _emailVerificationService.VerifyEmailAsync(req.RequestId, req.Code, ct);

    await SendAsync(new VerifyEmailResponse {
      Status = result.Status,
      Message = result.Message ?? "Email verified successfully"
    }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
  }
}
