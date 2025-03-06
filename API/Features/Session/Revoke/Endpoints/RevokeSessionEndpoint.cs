using FastEndpoints;
using API.Features.Session.Revoke.Interfaces;
using API.Features.Session.Revoke.Models.Contracts;

namespace API.Features.Session.Revoke.Endpoints;

public class RevokeSessionEndpoint :  Endpoint<RevokeSessionRequest>
{
    private readonly IRevokeSessionService _revokeSessionService;

    public RevokeSessionEndpoint(IRevokeSessionService revokeSessionService)
    {
        _revokeSessionService = revokeSessionService;
    }

    public override void Configure()
    {
        Post("/api/session/revoke");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RevokeSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _revokeSessionService.RevokeSessionAsync(long.Parse(request.SessionId));

        await SendAsync(new RevokeSessionResponse {
          Status = result.Status,
          Message = result.Message,
        });

    }
    
}
