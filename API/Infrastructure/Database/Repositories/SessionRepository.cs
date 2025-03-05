using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Database.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AuthDbContext _context;

    public SessionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddSessionAsync(Session session)
    {
        try
        {
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to add session: {ex.Message}", ex);
        }
    }

    public async Task AddTokenAsync(Token token)
    {
        try
        {
            await _context.Tokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to add token: {ex.Message}", ex);
        }
    }

    public async Task<Session?> GetSessionByIdAsync(long id, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false)
    {
        try
        {
            IQueryable<Session> query = _context.Sessions;

            if (includeUser) {
                query = query.Include(s => s.User);
            }
            if (includeAccount) {
                query = query.Include(s => s.Account);
            }
            if (includeApplication) {
                query = query.Include(s => s.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(s => s.ApplicationAccount);
            }
            if (includeTokens) {
                query = query.Include(s => s.Tokens);
            }
            
            return await query.FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get session by id: {ex.Message}", ex);
        }
    }

    public async Task<Session?> GetSessionByTokenStringAsync(string tokenString, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false)
    {
        try
        {
            IQueryable<Session> query = _context.Sessions;

            if (includeUser) {
                query = query.Include(s => s.User);
            }
            if (includeAccount) {
                query = query.Include(s => s.Account);
            }
            if (includeApplication) {
                query = query.Include(s => s.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(s => s.ApplicationAccount);
            }
            if (includeTokens) {
                query = query.Include(s => s.Tokens);
            }
            
            return await query.FirstOrDefaultAsync(s => s.Tokens.Any(t => t.Value == tokenString));
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get session by token string: {ex.Message}", ex);
        }
    }

    public async Task<Session?> GetSessionByUserIdAsync(long userId, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false)
    {
        try
        {
            IQueryable<Session> query = _context.Sessions;

            if (includeUser) {
                query = query.Include(s => s.User);
            }
            if (includeAccount) {
                query = query.Include(s => s.Account);
            }
            if (includeApplication) {
                query = query.Include(s => s.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(s => s.ApplicationAccount);
            }
            if (includeTokens) {
                query = query.Include(s => s.Tokens);
            }
            
            return await query.FirstOrDefaultAsync(s => s.UserId == userId);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get session by user id: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Session>> GetAllUserSessionsAsync(long userId, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false)
    {
        try
        {
            IQueryable<Session> query = _context.Sessions;

            if (includeUser) {
                query = query.Include(s => s.User);
            }
            if (includeAccount) {
                query = query.Include(s => s.Account);
            }
            if (includeApplication) {
                query = query.Include(s => s.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(s => s.ApplicationAccount);
            }
            if (includeTokens) {
                query = query.Include(s => s.Tokens);
            }
            
            return await query.Where(s => s.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get all user sessions: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Session>> GetActiveUserSessionsAsync(long userId, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false)
    {
        try
        {
            IQueryable<Session> query = _context.Sessions;

            if (includeUser) {
                query = query.Include(s => s.User);
            }
            if (includeAccount) {
                query = query.Include(s => s.Account);
            }
            if (includeApplication) {
                query = query.Include(s => s.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(s => s.ApplicationAccount);
            }
            if (includeTokens) {
                query = query.Include(s => s.Tokens);
            }
            
            return await query.Where(s => s.UserId == userId && !s.IsRevoked).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get active user sessions: {ex.Message}", ex);
        }
    }

    public async Task<Token?> GetTokenByTokenStringAsync(string tokenString, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false)
    {
        try
        {
            IQueryable<Token> query = _context.Tokens;

            if (includeSession) {
                query = query.Include(t => t.Session);
            }
            if (includeUser) {
                query = query.Include(t => t.User);
            }
            if (includeAccount) {
                query = query.Include(t => t.Account);
            }
            if (includeApplication) {
                query = query.Include(t => t.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(t => t.ApplicationAccount);
            }
            if (includeParentToken) {
                query = query.Include(t => t.ParentToken);
            }
            
            return await query.FirstOrDefaultAsync(t => t.Value == tokenString);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get token by token string: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Token>> GetAllUserTokensAsync(long userId, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false)
    {
        try
        {
            IQueryable<Token> query = _context.Tokens;

            if (includeSession) {
                query = query.Include(t => t.Session);
            }
            if (includeUser) {
                query = query.Include(t => t.User);
            }
            if (includeAccount) {
                query = query.Include(t => t.Account);
            }
            if (includeApplication) {
                query = query.Include(t => t.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(t => t.ApplicationAccount);
            }
            if (includeParentToken) {
                query = query.Include(t => t.ParentToken);
            }
            
            return await query.Where(t => t.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get all user tokens: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Token>> GetActiveUserTokensAsync(long userId, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false)
    {
        try
        {
            IQueryable<Token> query = _context.Tokens;

            if (includeSession) {
                query = query.Include(t => t.Session);
            }
            if (includeUser) {
                query = query.Include(t => t.User);
            }
            if (includeAccount) {
                query = query.Include(t => t.Account);
            }   
            if (includeApplication) {
                query = query.Include(t => t.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(t => t.ApplicationAccount);
            }
            if (includeParentToken) {
                query = query.Include(t => t.ParentToken);
            }
            
            return await query.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get active user tokens: {ex.Message}", ex);
        }
    }

    public async Task<Token?> GetTokenByIdAsync(long id, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false)
    {
        try
        {
            IQueryable<Token> query = _context.Tokens;

            if (includeSession) {
                query = query.Include(t => t.Session);
            }
            if (includeUser) {
                query = query.Include(t => t.User);
            }
            if (includeAccount) {
                query = query.Include(t => t.Account);
            }
            if (includeApplication) {
                query = query.Include(t => t.Application);
            }
            if (includeApplicationAccount) {
                query = query.Include(t => t.ApplicationAccount);
            }
            if (includeParentToken) {
                query = query.Include(t => t.ParentToken);
            }
            
            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get token by id: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetUserBySessionIdAsync(long sessionId, bool includeAccounts = false, bool includeSessions = false, bool includePhoneNumbers = false, bool includeEmailAddresses = false)
    {
        try
        {
            IQueryable<User> query = _context.Users;

            if (includeAccounts) {
                query = query.Include(u => u.Accounts);
            }
            if (includeSessions) {
                query = query.Include(u => u.Sessions);
            }
            if (includePhoneNumbers) {
                query = query.Include(u => u.PhoneNumbers);
            }
            if (includeEmailAddresses) {
                query = query.Include(u => u.EmailAddresses);
            }
            
            return await query.FirstOrDefaultAsync(u => u.Sessions.Any(s => s.Id == sessionId));
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get user by session id: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetUserByTokenIdAsync(long tokenId, bool includeAccounts = false, bool includeSessions = false, bool includePhoneNumbers = false, bool includeEmailAddresses = false)
    {
        try
        {
            var session = await _context.Sessions
                .Include(s => s.Tokens)
                .FirstOrDefaultAsync(s => s.Tokens.Any(t => t.Id == tokenId));

            if (session == null)
            {
                return null;
            }

            IQueryable<User> query = _context.Users.Where(u => u.Id == session.UserId);

            if (includeAccounts) {
                query = query.Include(u => u.Accounts);
            }
            if (includeSessions) {
                query = query.Include(u => u.Sessions);
            }
            if (includePhoneNumbers) {
                query = query.Include(u => u.PhoneNumbers);
            }
            if (includeEmailAddresses) {
                query = query.Include(u => u.EmailAddresses);
            }
            
            return await query.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get user by token id: {ex.Message}", ex);
        }
    }

    public async Task RevokeSessionAsync(long sessionId)
    {
        try
        {
            var session = await GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                throw new Exception("NOT_FOUND: Session not found");
            }
            session.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to revoke session: {ex.Message}", ex);
        }
    }

    public async Task RevokeAllUserSessionsAsync(long userId)
    {
        try
        {
            var sessions = await GetAllUserSessionsAsync(userId);
            foreach (var session in sessions)
            {
                session.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to revoke all user sessions: {ex.Message}", ex);
        }
    }

    public async Task RevokeAllSessionTokensAsync(long sessionId)
    {
        try
        {
            var session = await GetSessionByIdAsync(sessionId);
            if (session is null) {
                throw new Exception("NOT_FOUND: Session not found");
            }
            var tokens = await _context.Tokens.Where(t => t.SessionId == sessionId).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to revoke all session tokens: {ex.Message}", ex);
        }
    }

    public async Task RemoveTokenAsync(long tokenId)
    {
        try
        {
            var token = await GetTokenByIdAsync(tokenId);
            if (token == null)
            {
                throw new Exception("NOT_FOUND: Token not found");
            }
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to remove token: {ex.Message}", ex);
        }
    }

    public async Task RemoveSessionAsync(long sessionId)
    {
        try
        {
            var session = await GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                throw new Exception("NOT_FOUND: Session not found");
            }
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to remove session: {ex.Message}", ex);
        }
    }
}

