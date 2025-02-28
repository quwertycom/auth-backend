using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Shared.Configuration;
using API.Shared.Enums.Entities.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using API.Shared.Interfaces.Security;

namespace API.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        ValidateSettings();
    }

    private void ValidateSettings()
    {
        if (Encoding.UTF8.GetBytes(_settings.SecretKey).Length < 32)
            throw new InvalidOperationException("JWT secret key must be at least 32 bytes");
    }

    /// <summary>
    /// Generates a refresh token for the given target and IDs.
    /// </summary>
    public virtual (bool isSuccess, string status, string message, string? token) GenerateRefreshToken(TokenTarget target, (long userId, long? accountId, long? applicationId) ids)
    {
        try
        {
            // Validate required IDs based on target
            switch (target)
            {
                case TokenTarget.User when string.IsNullOrEmpty(ids.userId.ToString()):
                    return (false, "ERROR", "Missing user id", null);

                case TokenTarget.Account when !ids.accountId.HasValue || string.IsNullOrEmpty(ids.userId.ToString()):
                    return (false, "ERROR", "Missing account id or user id", null);

                case TokenTarget.Application when !ids.applicationId.HasValue || !ids.accountId.HasValue || string.IsNullOrEmpty(ids.userId.ToString()):
                    return (false, "ERROR", "Missing application id, account id, or user id", null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

            // Generate a random value for jti (JWT ID)
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var jti = Convert.ToBase64String(randomBytes);

            // Base claims that exist in all token types
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, jti),
                new("token_type", "refresh"),
                new("token_target", target.ToString())
            };

            // Add specific claims based on token target
            switch (target)
            {
                case TokenTarget.User:
                    claims.Add(new(JwtRegisteredClaimNames.Sub, ids.userId.ToString()));
                    claims.Add(new("user_id", ids.userId.ToString()));
                    break;

                case TokenTarget.Account:
                    claims.Add(new(JwtRegisteredClaimNames.Sub, ids.accountId!.Value.ToString()));
                    claims.Add(new("user_id", ids.userId.ToString()));
                    break;

                case TokenTarget.Application:
                    claims.Add(new(JwtRegisteredClaimNames.Sub, ids.applicationId!.Value.ToString()));
                    claims.Add(new("user_id", ids.userId.ToString()));
                    claims.Add(new("account_id", ids.accountId!.Value.ToString()));
                    break;

                default:
                    return (false, "ERROR", "Invalid token target", null);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (true, "SUCCESS", "Token generated successfully", tokenHandler.WriteToken(token));
        }
        catch (Exception ex)
        {
            return (false, "ERROR", $"Failed to generate refresh token: {ex.Message}", null);
        }
    }

    public virtual (bool isSuccess, string status, string message, string? token) GenerateAccessToken(string refreshToken)
    {
        try
        {
            var claims = GetTokenClaims(refreshToken);
            if (claims == null)
            {
                return (false, "ERROR", "Invalid refresh token", null);
            }

            // Verify it's a refresh token
            if (!claims.TryGetValue("token_type", out var tokenType) || tokenType != "refresh")
            {
                return (false, "ERROR", "Invalid token type", null);
            }

            // Get token target
            if (!claims.TryGetValue("token_target", out var targetStr) ||
                !Enum.TryParse<TokenTarget>(targetStr, out var target))
            {
                return (false, "ERROR", "Invalid token target", null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

            // Generate a random value for jti (JWT ID)
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var jti = Convert.ToBase64String(randomBytes);

            // Base claims that exist in all token types
            var accessClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, claims[JwtRegisteredClaimNames.Sub]),
                new(JwtRegisteredClaimNames.Jti, jti),
                new("token_type", "access"),
                new("token_target", targetStr)
            };

            // Add specific claims based on token target
            switch (target)
            {
                case TokenTarget.User:
                    accessClaims.Add(new(JwtRegisteredClaimNames.Sub, claims[JwtRegisteredClaimNames.Sub]));
                    accessClaims.Add(new("user_id", claims[JwtRegisteredClaimNames.Sub]));
                    break;

                case TokenTarget.Account:
                    accessClaims.Add(new(JwtRegisteredClaimNames.Sub, claims[JwtRegisteredClaimNames.Sub]));
                    accessClaims.Add(new("user_id", claims["user_id"]));
                    break;

                case TokenTarget.Application:
                    accessClaims.Add(new(JwtRegisteredClaimNames.Sub, claims[JwtRegisteredClaimNames.Sub]));
                    accessClaims.Add(new("user_id", claims["user_id"]));
                    accessClaims.Add(new("account_id", claims["account_id"]));
                    break;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(accessClaims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Access tokens have shorter lifetime
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (true, "SUCCESS", "Access token generated successfully", tokenHandler.WriteToken(token));
        }
        catch (Exception ex)
        {
            return (false, "ERROR", $"Failed to generate access token: {ex.Message}", null);
        }
    }

    public virtual bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            claimsPrincipal = null;
            return false;
        }
    }

    private IDictionary<string, string>? GetTokenClaims(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                return null;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            return claims;
        }
        catch
        {
            return null;
        }
    }
}