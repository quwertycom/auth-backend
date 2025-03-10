using FastEndpoints;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Interfaces;
using API.Shared.Contracts.Responses.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using API.Features.User.Password.Reset.Models.Services;

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
        Post("/api/user/password/reset/request");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RequestPasswordResetRequest req, CancellationToken ct)
    {
        RequestPasswordResetResult result;

        if (!string.IsNullOrEmpty(req.Email))
        {
            result = await _resetPasswordService.RequestPasswordResetViaEmailAsync(req.Email, ct);
        }
        else if (!string.IsNullOrEmpty(req.Username))
        {
            result = await _resetPasswordService.RequestPasswordResetViaUsernameAsync(req.Username, ct);
            
            await SendAsync(new RequestPasswordResetResponse
            {
                Status = result.Status,
                Message = result.Message
            }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
        }
    }
}