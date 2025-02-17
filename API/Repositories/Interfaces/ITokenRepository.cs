using API.Models;

namespace API.Repositories.Interfaces;

public interface ITokenRepository
{
    public Task AddToken(Token token);
}