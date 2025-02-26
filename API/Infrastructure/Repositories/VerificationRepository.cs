using API.Data;
using API.Models;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class VerificationRepository : IVerificationRepository
{
    private readonly AuthDbContext _Context;
    public VerificationRepository(AuthDbContext context)
    {
        _Context = context;
    }
    public async Task AddVerificationSession(VerificationSession session)
    {
        await _Context.VerificationSessions.AddAsync(session);
        await _Context.SaveChangesAsync();
    }

    public async Task<VerificationSession?> GetVerificationSessionById(long verificationSessionID)
    {
        return await _Context.VerificationSessions
            .Where(vs => vs.Id == verificationSessionID)
            .Include(vs => vs.User)
            .Include(vs => vs.Email)
            .FirstOrDefaultAsync();
    }
    public async Task<VerificationSession?> GetVerificationSessionByCode(string code)
    {
        return await _Context.VerificationSessions
            .FirstOrDefaultAsync(vs => vs.Code == code);
    }

    public async Task UpdateVerificationSession(VerificationSession session)
    {
        _Context.VerificationSessions.Update(session);
        await _Context.SaveChangesAsync();
    }

    public async Task AddResetPasswordRequest(ResetPasswordRequest request)
    {
        await _Context.ResetPasswordRequests.AddAsync(request);
        await _Context.SaveChangesAsync();
    }

    public async Task<ResetPasswordRequest?> GetResetPasswordRequestByCodeHash(string codeHash)
    {
        return await _Context.ResetPasswordRequests
            .Include(rpr => rpr.User)
            .Include(rpr => rpr.EmailAddress)
            .FirstOrDefaultAsync(x => x.CodeHash == codeHash);
    }
    public async Task UpdateResetPasswordRequest(ResetPasswordRequest request)
    {
        _Context.ResetPasswordRequests.Update(request);
        await _Context.SaveChangesAsync();
    }
}
