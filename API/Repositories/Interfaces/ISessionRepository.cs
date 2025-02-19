using API.Models;

namespace API.Repositories.Interfaces;

public interface ISessionRepository
{
    public Task AddSession(Session session);
    public Task AddSession(VerificationSession session);
}
