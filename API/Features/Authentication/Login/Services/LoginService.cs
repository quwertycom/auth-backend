using API.Infrastructure.Database.Entities;
using API.Features.Authentication.Login.Interfaces;
using API.Features.Authentication.Login.Models.Services;
using API.Infrastructure.Database.Entities.Authentication;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Database.Repositories;
using Session = API.Infrastructure.Database.Entities.Authentication.Session;
namespace API.Features.Authentication.Login.Services;


public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IHasher _hasher;
    private readonly IJwtService _jwtService;

    public LoginService(IUserRepository userRepository, ISessionRepository sessionRepository, IHasher hasher, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _hasher = hasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Invalid credentials",
                    HttpStatusCode = 401
                };
            }

            if (user.State != UserState.Active)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "User is not active",
                    HttpStatusCode = 401
                };
            }
            else if (!_hasher.Compare(password, user.PasswordHash, user.PasswordSalt).IsMatch)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Invalid credentials",
                    HttpStatusCode = 401
                };
            }

            var refreshTokenResult = _jwtService.GenerateRefreshToken(TokenTarget.User, (user.Id, null, null));

            if (!refreshTokenResult.IsSuccess || refreshTokenResult.RefreshToken == null)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = refreshTokenResult.Message ?? "Internal server error",
                    HttpStatusCode = 500
                };
            }

            var accessTokenResult = _jwtService.GenerateAccessToken(refreshTokenResult.RefreshToken);

            if (!accessTokenResult.IsSuccess || accessTokenResult.AccessToken == null)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = accessTokenResult.Message ?? "Internal server error",
                    HttpStatusCode = 500
                };
            }

            var newSession = new Infrastructure.Database.Entities.Authentication.Session
            {
                User = user,
                Target = SessionTarget.User,
                Tokens = new List<Token> { }
            };

            var refreshToken = new Token
            {
                Value = refreshTokenResult.RefreshToken,
                Type = TokenType.Refresh,
                Target = TokenTarget.User,
                User = user,
                Session = newSession
            };

            var accessToken = new Token
            {
                Value = accessTokenResult.AccessToken,
                Type = TokenType.Access,
                Target = TokenTarget.User,
                User = user,
                Session = newSession,
                ParentToken = refreshToken
            };

            await _sessionRepository.AddSessionAsync(newSession);
            await _sessionRepository.AddTokenAsync(refreshToken);
            await _sessionRepository.AddTokenAsync(accessToken);

            return new LoginResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "User logged in successfully",
                HttpStatusCode = 200,
                RefreshToken = refreshTokenResult.RefreshToken,
                AccessToken = accessTokenResult.AccessToken
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = ex.Message ?? "Internal server error",
                HttpStatusCode = 500
            };
        }
    }
}