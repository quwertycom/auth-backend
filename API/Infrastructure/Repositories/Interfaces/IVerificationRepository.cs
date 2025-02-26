using API.Core.Models;

namespace API.Infrastructure.Repositories.Interfaces;

public interface IVerificationRepository
{
    public Task AddVerificationSession(VerificationSession session);
    public Task<VerificationSession?> GetVerificationSessionById(long verificationSessionID);
    public Task<VerificationSession?> GetVerificationSessionByCode(string code);
    public Task UpdateVerificationSession(VerificationSession session);
    public Task AddResetPasswordRequest(ResetPasswordRequest request);
    public Task<ResetPasswordRequest?> GetResetPasswordRequestByCodeHash(string codeHash);
    public Task UpdateResetPasswordRequest(ResetPasswordRequest request);
}
