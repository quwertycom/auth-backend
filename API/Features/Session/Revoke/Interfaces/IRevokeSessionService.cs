using API.Features.Session.Revoke.Models.Services;

namespace API.Features.Session.Revoke.Interfaces;

public interface IRevokeSessionService
{
    Task<RevokeSessionResult> RevokeSessionAsync(long sessionId);
}
