using System.Security.Claims;
using API.Shared.Enums.Entities.Authentication;

namespace API.Shared.Interfaces.Security;

/// <summary>
/// Interface for JWT service.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <param name="target">The token target.</param>
    /// <param name="ids">The IDs associated with the token.</param>
    /// <returns>A tuple indicating success, status, message, and the token (if successful).</returns>
    (bool isSuccess, string status, string message, string? token) GenerateRefreshToken(TokenTarget target, (long userId, long? accountId, long? applicationId) ids);

    /// <summary>
    /// Generates an access token from a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>A tuple indicating success, status, message, and the access token (if successful).</returns>
    (bool isSuccess, string status, string message, string? token) GenerateAccessToken(string refreshToken);

    /// <summary>
    /// Validates a token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="claimsPrincipal">The claims principal if the token is valid, otherwise null.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal);
}

