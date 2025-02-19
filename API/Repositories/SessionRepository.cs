using API.Data;
using API.Models;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AuthDbContext _Context;
    private readonly IVerificationRepository _verificationRepository;
    public SessionRepository(AuthDbContext context, IVerificationRepository verificationRepository)
    {
        _Context = context;
        _verificationRepository = verificationRepository;
    }

    public async Task AddSession(Session session)
    {
        try
        {
            await _Context.Sessions.AddAsync(session);
            await _Context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
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

    public async Task<string> GetSessionState(long sessionId)
    {
        try
        {
            var session = await GetSessionById(sessionId);
            if (session == null)
                throw new Exception("NOT_FOUND");
            else if (session.IsRevoked)
                return "REVOKED";
            else
                return "ACTIVE";
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Session?> GetSessionById(long sessionId)
    {
        try
        {
            return await _Context.Sessions
                .Include(s => s.Tokens)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Session?> GetSessionByTokenString(string tokenString)
    {
        try
        {
            return await _Context.Sessions.FirstOrDefaultAsync(s => s.Tokens.Any(t => t.TokenString == tokenString));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Session?> GetSessionByTokenId(long tokenId)
    {
        try
        {
            return await _Context.Sessions.FirstOrDefaultAsync(s => s.Tokens.Any(t => t.Id == tokenId));
        }
        catch (Exception)
        {
            throw;
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

    public async Task RevokeSession(long sessionId)
    {
        try
        {
            var sessionToRevoke = await _Context.Sessions
                .Include(s => s.Tokens)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (sessionToRevoke == null)
            {
                throw new Exception("NOT_FOUND");
            }

            var tokensToRevoke = await _Context.Tokens
                .Where(t => t.SessionId == sessionId && !t.IsRevoked && !t.IsRefreshed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in tokensToRevoke)
            {
                token.IsRevoked = true;
            }

            sessionToRevoke.IsRevoked = true;
            await _Context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
