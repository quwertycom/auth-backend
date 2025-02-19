using API.Data;
using API.Models;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        await _Context.Sessions.AddAsync(session);
        await _Context.SaveChangesAsync();
    }
    public async Task AddSession(VerificationSession session)
    {
        await _Context.VerificationSessions.AddAsync(session);
        await _Context.SaveChangesAsync();
    }
}
