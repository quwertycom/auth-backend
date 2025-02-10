using API.Data;
using API.Models;

namespace API.Service;

public interface ITokenRepository
{
    public Task AddToken(Token token);
}
public class TokenRepository : ITokenRepository
{
    private readonly AuthDbContext _Context;
    public TokenRepository(AuthDbContext context)
    {
        _Context = context;
    }
    public async Task AddToken(Token token)
    {
        await _Context.Tokens.AddAsync(token);
        await _Context.SaveChangesAsync();
    }
}
