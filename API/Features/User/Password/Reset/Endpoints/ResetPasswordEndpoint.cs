using FastEndpoints;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Interfaces;

namespace API.Features.User.Password.Reset.Endpoints;

public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest, ResetPasswordResponse>
{
    private readonly IResetPasswordService _resetPasswordService;

    public ResetPasswordEndpoint(IResetPasswordService resetPasswordService)
    {
        _resetPasswordService = resetPasswordService;
    }

    public override void Configure()
    {
        Post("/api/user/password/reset");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var result = await _resetPasswordService.ResetPasswordAsync(req.Code, req.NewPassword, ct);

        await SendAsync(new ResetPasswordResponse {
            Status = result.Status,
            Message = result.Message
        }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
    }
}