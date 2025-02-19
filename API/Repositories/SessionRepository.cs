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
        try {
            await _Context.Sessions.AddAsync(session);
            await _Context.SaveChangesAsync();
        } catch (Exception) {
            throw;
        }
    }

    public async Task<Session?> GetSessionById(long sessionId)
    {
        try {
            return await _Context.Sessions.FindAsync(sessionId);
        } catch (Exception) {
            throw;
        }
    }

    public async Task<Session?> GetSessionByTokenString(string tokenString)
    {
        try {
            return await _Context.Sessions.FirstOrDefaultAsync(s => s.Tokens.Any(t => t.TokenString == tokenString));
        } catch (Exception) {
            throw;
        }
    }

    public async Task<Session?> GetSessionByTokenId(long tokenId)
    {
        try {
            return await _Context.Sessions.FirstOrDefaultAsync(s => s.Tokens.Any(t => t.Id == tokenId));
        } catch (Exception) {
            throw;
        }
    }

    public async Task RevokeSession(long sessionId)
    {
        try {
            var session = await GetSessionById(sessionId);
            if (session == null) 
                throw new Exception("NOT_FOUND");
            else if (session.IsRevoked)
                throw new Exception("ALREADY_REVOKED");
            
            session.IsRevoked = true;
            await _Context.SaveChangesAsync();
        } catch (Exception) {
            throw;
        }
    }

    public async Task<string> GetSessionState(long sessionId)
    {
        try {
            var session = await GetSessionById(sessionId);
            if (session == null)
                throw new Exception("NOT_FOUND");
            else if (session.IsRevoked)
                return "REVOKED";
            else
                return "ACTIVE";
        } catch (Exception) {
            throw;
        }
    }
}
