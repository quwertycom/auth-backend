using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public interface ISessionRepository
    {
        public Task AddSession(Session session);
        public Task<VerificationSession> GetSeession(long VerificationSessionID);
        public Task AddSession(VerificationSession session);
    }
    public class SessionRepository : ISessionRepository
    {
        private readonly AuthDbContext _Context;
        public SessionRepository(AuthDbContext context)
        {
            _Context = context;
        }
        public async Task AddSession(Session session)
        {
            await _Context.Sessions.AddAsync(session);
            await _Context.SaveChangesAsync();
        }
        public async Task<VerificationSession> GetSeession(long VerificationSessionID)
        {
            return await _Context.VerificationSessions
            .Where(vs => vs.Id == VerificationSessionID)
            .Include(vs => vs.User)
            .Include(vs => vs.Email)
            .FirstOrDefaultAsync();
        }
        public async Task AddSession(VerificationSession session)
        {
            await _Context.VerificationSessions.AddAsync(session);
            await _Context.SaveChangesAsync();
        }
    }
}
