using API.Data;
using API.Models;
using API.Repositories.Interfaces;

namespace API.Repositories;

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