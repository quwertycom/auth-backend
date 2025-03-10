using FastEndpoints;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Interfaces;
using API.Shared.Contracts.Responses.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Features.User.Password.Reset.Endpoints;

public class RequestPasswordResetEndpoint : Endpoint<RequestPasswordResetRequest, RequestPasswordResetResponse>
{
    private readonly IResetPasswordService _resetPasswordService;

    public RequestPasswordResetEndpoint(IResetPasswordService resetPasswordService)
    {
        _resetPasswordService = resetPasswordService;
    }

    public override void Configure()
    {
        Post("/api/user/password/reset");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RequestPasswordResetRequest req, CancellationToken ct)
    {
        var result = await _resetPasswordService.RequestPasswordResetViaEmailAsync(req.Email, ct);

        await SendAsync(new RequestPasswordResetResponse
        {
            Status = result.Status,
            Message = result.Message
        }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
    }
}