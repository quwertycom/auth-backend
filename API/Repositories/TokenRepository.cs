using API.Data;
using API.Models;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace API.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly AuthDbContext _Context;
    public TokenRepository(AuthDbContext context)
    {
        _Context = context;
    }

    public async Task AddToken(Token token)
    {
        try
        {
            await _Context.Tokens.AddAsync(token);
            await _Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<Token?> GetTokenByTokenString(string tokenString)
    {
        try
        {
            return await _Context.Tokens.FirstOrDefaultAsync(t => t.TokenString == tokenString);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<Token?> GetTokenByUserId(long userId)
    {
        try
        {
            return await _Context.Tokens.FirstOrDefaultAsync(t => t.UserId == userId);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<IEnumerable<Token>> GetSessionTokensBySessionId(long sessionId)
    {
        try
        {
            return await _Context.Tokens.Where(t => t.SessionId == sessionId).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}