using FastEndpoints;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using API.Features.Authentication.EmailVerification.Interfaces;

namespace API.Features.Authentication.EmailVerification.Endpoints;

public class RequestStatusEndpoint : Endpoint<RequestStatusRequest, RequestStatusResponse>
{
    private readonly IEmailVerificationService _emailVerificationService;

    public RequestStatusEndpoint(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    public override void Configure()
    {
        Get("/api/authentication/email-verification/request-status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RequestStatusRequest req, CancellationToken ct)
    {
        var result = await _emailVerificationService.GetRequestStatusAsync(req.RequestId, req.Email, ct);

        if (result.IsSuccess) {
            await SendAsync(new RequestStatusResponse {
                Status = result.Status,
                Message = result.Message
            }, statusCode: result.HttpStatusCode ?? 200, ct);
        } else {
            await SendAsync(new RequestStatusResponse {
                Status = result.Status,
                Message = result.Message
            }, statusCode: result.HttpStatusCode ?? 400, ct);
        }
    }
}