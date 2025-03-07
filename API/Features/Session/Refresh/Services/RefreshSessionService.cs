using API.Features.Session.Refresh.Interfaces;
using API.Features.Session.Refresh.Models.Services;
using API.Infrastructure.Database.Entities.Authentication;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
namespace API.Features.Session.Refresh.Services;

public class RefreshSessionService : IRefreshSessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IJwtService _jwtService;
    public RefreshSessionService(ISessionRepository sessionRepository, IJwtService jwtService)
    {
        _sessionRepository = sessionRepository;
        _jwtService = jwtService;
    }

    public async Task<RefreshSessionResult> RefreshSessionAsync(string refreshToken)
    {
        try
        {
            var session = await _sessionRepository.GetSessionByTokenStringAsync(refreshToken, includeUser: true);

            if (session == null || session.User == null)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Session not found",
                    HttpStatusCode = 404
                };
            }
            else if (session.IsRevoked)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Session has been already revoked",
                    HttpStatusCode = 400
                };
            }

            var token = await _sessionRepository.GetTokenByTokenStringAsync(refreshToken);

            if (token == null)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Token not found",
                    HttpStatusCode = 404
                };
            } else if (token.Type != TokenType.Refresh)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Token is not a refresh token",
                    HttpStatusCode = 400
                };
            } else if (token.IsRefreshed)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Token has already been refreshed",
                    HttpStatusCode = 400
                };
            } else if (token.IsRevoked)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Token has already been revoked",
                    HttpStatusCode = 400
                };
            }

            var tokenTarget = session.Target == SessionTarget.User ? TokenTarget.User
                            : session.Target == SessionTarget.Account ? TokenTarget.Account
                            : TokenTarget.Application;

            var getNewTokensResult = _jwtService.RefreshTokens(refreshToken);

            if (!getNewTokensResult.IsSuccess)
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = getNewTokensResult.Status,
                    Message = getNewTokensResult.Message ?? "Internal server error",
                    HttpStatusCode = 500
                };
            } else if (string.IsNullOrEmpty(getNewTokensResult.AccessToken) || string.IsNullOrEmpty(getNewTokensResult.RefreshToken))
            {
                return new RefreshSessionResult {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Internal server error",
                    HttpStatusCode = 500
                };
            }

            var newRefreshToken = new Token {
                Value = getNewTokensResult.RefreshToken,
                Type = TokenType.Refresh,
                Target = tokenTarget,
                User = session.User,
                Session = session
            };

            var newAccessToken = new Token {
                Value = getNewTokensResult.AccessToken,
                Type = TokenType.Access,
                Target = tokenTarget,
                User = session.User,
                Session = session
            };

            await _sessionRepository.RevokeAllSessionTokensAsync(session.Id);

            await _sessionRepository.AddTokenAsync(newRefreshToken);
            await _sessionRepository.AddTokenAsync(newAccessToken);

            return new RefreshSessionResult {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Session refreshed",
                AccessToken = getNewTokensResult.AccessToken,
                RefreshToken = getNewTokensResult.RefreshToken
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new RefreshSessionResult
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = ex.Message ?? "Internal server error",
                HttpStatusCode = 500
            };
        }
    }
}