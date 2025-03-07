using FastEndpoints;
using API.Features.Session.Refresh.Interfaces;
using API.Features.Session.Refresh.Models.Contracts;
using API.Features.Session.Refresh.Models.Services;
using Microsoft.AspNetCore.Http;
namespace API.Features.Session.Refresh.Endpoints;

public class RefreshSessionEndpoint : Endpoint<RefreshSessionRequest>
{
    private readonly IRefreshSessionService _refreshSessionService;

    public RefreshSessionEndpoint(IRefreshSessionService refreshSessionService)
    {
        _refreshSessionService = refreshSessionService;
    }

    public override void Configure()
    {
        Post("/api/session/refresh");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _refreshSessionService.RefreshSessionAsync(request.Token);

        if (result.HttpStatusCode == 204)
        {
            await SendNoContentAsync(cancellationToken);
            return;
        }

        await SendAsync(new RefreshSessionResponse
        {
            Status = result.Status,
            Message = result.Message,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
        }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), cancellationToken);
    }

}
