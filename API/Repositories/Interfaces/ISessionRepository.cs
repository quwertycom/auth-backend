using API.Models;

namespace API.Repositories.Interfaces;

public interface ISessionRepository
{
    public Task AddSession(Session session);
    public Task<Session?> GetSessionById(long sessionId);
    public Task<Session?> GetSessionByTokenString(string tokenString);
    public Task<Session?> GetSessionByTokenId(long tokenId);
    public Task RevokeSession(long sessionId);
    public Task<string> GetSessionState(long sessionId);
    public Task AddToken(Token token);
    public Task<Token?> GetTokenByTokenString(string tokenString);
    public Task<Token?> GetTokenByUserId(long userId);
    public Task<IEnumerable<Token>> GetSessionTokensBySessionId(long sessionId);
}
