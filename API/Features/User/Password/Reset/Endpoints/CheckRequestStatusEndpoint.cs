using FastEndpoints;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Interfaces;

namespace API.Features.User.Password.Reset.Endpoints;

public class CheckRequestStatusEndpoint : Endpoint<CheckRequestStatusRequest, CheckRequestStatusResponse>
{
    private readonly IResetPasswordService _resetPasswordService;

    public CheckRequestStatusEndpoint(IResetPasswordService resetPasswordService)
    {
        _resetPasswordService = resetPasswordService;
    }

    public override void Configure()
    {
        Get("/api/user/password/reset/request-status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CheckRequestStatusRequest req, CancellationToken ct)
    {
        var result = await _resetPasswordService.CheckRequestStatusAsync(req.Code, ct);

        await SendAsync(new CheckRequestStatusResponse
        {
            Status = result.Status,
            Message = result.Message,
            IsExpired = result.IsExpired,
            IsUsed = result.IsUsed
        }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
    }
}
