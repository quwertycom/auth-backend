using API.Services.Interfaces;
using API.Repositories.Interfaces;
namespace API.Services;

public class TokenService : ITokenService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ISessionRepository _sessionRepository;

    public TokenService(IUserRepository userRepository, ITokenRepository tokenRepository, ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<(bool isSuccess, string status, string message, bool isValid)> ValidateAsync(string token)
    {
      // TODO: Replace it with actual logic
      return (true, "success", "Token is valid", true);
    }
    
}
