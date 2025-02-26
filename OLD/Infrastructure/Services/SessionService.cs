using API.Infrastructure.Repositories.Interfaces;
using API.Core.Services.Interfaces;

namespace API.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<(bool isSuccess, string status, string message)> RevokeSessionByToken(string token)
    {
        try
        {
            var tokenSession = await _sessionRepository.GetSessionByTokenString(token);
            if (tokenSession == null)
                return (false, "NOT_FOUND", "Session not found.");
            else if (tokenSession.IsRevoked)
                return (false, "ALREADY_REVOKED", "Session already revoked.");

            await _sessionRepository.RevokeSession(tokenSession.Id);
            return (true, "SUCCESS", "Session revoked successfully.");
        }
        catch (Exception)
        {
            return (false, "INTERNAL_ERROR", "Internal server error, please try again later, if issue persists contact support.");
        }


    }
}