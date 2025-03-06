using API.Features.Session.Revoke.Interfaces;
using API.Features.Session.Revoke.Models.Services;
using API.Shared.Interfaces.Database.Repositories;

namespace API.Features.Session.Revoke.Services;

public class RevokeSessionService : IRevokeSessionService
{
    private readonly ISessionRepository _sessionRepository;

    public RevokeSessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<RevokeSessionResult> RevokeSessionAsync(long sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetSessionByIdAsync(sessionId);

            if (session == null)
            {
                return new RevokeSessionResult { IsSuccess = false, Status = "ERROR", Message = "Session not found", HttpStatusCode = 404 };
            } else if (session.IsRevoked) {
              return new RevokeSessionResult { IsSuccess = false, Status = "ERROR", Message = "Session has been already revoked", HttpStatusCode = 400 };
            }
            
            await _sessionRepository.RevokeSessionAsync(session.Id);

            return new RevokeSessionResult { IsSuccess = true, Status = "SUCCESS", Message = "Session revoked" };
        }
        catch (Exception ex)
        {
            return new RevokeSessionResult
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = ex.Message ?? "Internal server error",
                HttpStatusCode = 500
            };
        }
    }
}