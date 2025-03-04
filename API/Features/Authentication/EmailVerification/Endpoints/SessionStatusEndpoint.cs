using FastEndpoints;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Features.Authentication.EmailVerification.Interfaces;

namespace API.Features.Authentication.EmailVerification.Endpoints;

public class SessionStatusEndpoint : Endpoint<SessionStatusRequest, SessionStatusResponse>
{
    private readonly IEmailVerificationService _emailVerificationService;

    public SessionStatusEndpoint(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    public override void Configure()
    {
        Get("/api/authentication/email-verification/session-status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SessionStatusRequest req, CancellationToken ct)
    {
        var result = await _emailVerificationService.GetSessionStatusAsync(long.Parse(req.SessionId), req.Email, ct);

        if (result.IsSuccess) {
            await SendAsync(new SessionStatusResponse {
                Status = result.Status,
                Message = result.Message
            }, statusCode: result.HttpStatusCode ?? 200, ct);
        } else {
            await SendAsync(new SessionStatusResponse {
                Status = result.Status,
                Message = result.Message
            }, statusCode: result.HttpStatusCode ?? 400, ct);
        }
    }
}