using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.EmailVerification.Models.Contracts;
using FastEndpoints;

namespace API.Features.Authentication.EmailVerification.Endpoints;

public class RequestNewCodeEndpoint : Endpoint<RequestNewCodeRequest, RequestNewCodeResponse>
{
    private readonly IEmailVerificationService _emailVerificationService;

    public RequestNewCodeEndpoint(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    public override void Configure()
    {
        Post("/api/authentication/email-verification/request-new-code");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RequestNewCodeRequest req, CancellationToken ct)
    {
        var result = await _emailVerificationService.RequestNewCodeAsync(req.Email, ct);

        await SendAsync(new RequestNewCodeResponse
        {
            NewRequestId = result.NewRequestId,
            Status = result.Status,
            Message = result.Message,
        }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
    }
}
