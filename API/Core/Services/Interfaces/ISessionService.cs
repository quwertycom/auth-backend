
namespace API.Services.Interfaces;

public interface ISessionService
{
    public Task<(bool isSuccess, string status, string message)> RevokeSessionByToken(string token);
}