using API.Models;

namespace API.Repositories.Interfaces;

public interface ITokenRepository
{
    public Task AddToken(Token token);
    public Task<Token?> GetTokenByTokenString(string tokenString);
    public Task<Token?> GetTokenByUserId(long userId);
    public Task<IEnumerable<Token>> GetSessionTokensBySessionId(long sessionId);
}