using API.Services.Interfaces;
using API.Repositories.Interfaces;
using API.Common.Enums;

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

    public async Task<(bool isSuccess, string status, string message, bool isValid, TokenType? tokenType, TokenTarget? tokenTarget)> ValidateAsync(string token)
    {
        try
        {
            var tokenModel = await _tokenRepository.GetTokenByTokenString(token);
            if (tokenModel == null)
                return (false, "INVALID_TOKEN", "Invalid token.", false, null, null);
            else if (tokenModel.ExpiresAt < DateTime.UtcNow)
                return (false, "EXPIRED_TOKEN", "Token has expired.", false, tokenModel.Type, tokenModel.Target);
            else if (tokenModel.IsRefreshed || tokenModel.IsRevoked)
                return (false, "TOKEN_REVOKED", "Token has been revoked.", false, tokenModel.Type, tokenModel.Target);

            return (true, "SUCCESS", "Token is valid.", true, tokenModel.Type, tokenModel.Target);
        }
        catch (Exception ex)
        {
            return (false, "INTERNAL_ERROR", ex.Message ?? "Internal server error, please try again later, if issue persists contact support.", false, null, null);
        }
    }

}
