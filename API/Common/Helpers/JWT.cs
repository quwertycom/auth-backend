using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Common.Helpers;

public static class JWT
{
    private static string _secretKey = string.Empty;
    private static string _issuer = string.Empty;
    private static string _audience = string.Empty;
    private static bool _isInitialized = false;

    public static void Initialize(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (_isInitialized) return;

        if (environment.IsDevelopment())
        {
            _secretKey = configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
            _issuer = configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer is not configured");
            _audience = configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience is not configured");
        }
        else
        {
            _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set");
            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER environment variable is not set");
            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE environment variable is not set");
        }

        // Validate secret key length for HMAC SHA256
        if (Encoding.UTF8.GetBytes(_secretKey).Length < 32)
        {
            throw new InvalidOperationException("JWT secret key must be at least 32 bytes (256 bits) long");
        }

        _isInitialized = true;
    }

    public static (string status, string message, string? token) GenerateRefreshToken(TokenTarget target, (long userId, long? accountId, long? applicationId) ids)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("JWT helper is not initialized. Call Initialize() first.");
        }

        try
        {
            // Validate required IDs based on target
            switch (target)
            {
                case TokenTarget.User when string.IsNullOrEmpty(ids.userId.ToString()):
                    return ("ERROR", "Missing user id", null);

                case TokenTarget.Account when !ids.accountId.HasValue || string.IsNullOrEmpty(ids.userId.ToString()):
                    return ("ERROR", "Missing account id or user id", null);

                case TokenTarget.Application when !ids.applicationId.HasValue || !ids.accountId.HasValue || string.IsNullOrEmpty(ids.userId.ToString()):
                    return ("ERROR", "Missing application id, account id, or user id", null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

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
                    return ("ERROR", "Invalid token target", null);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return ("SUCCESS", "Token generated successfully", tokenHandler.WriteToken(token));
        }
        catch (Exception ex)
        {
            return ("ERROR", $"Failed to generate refresh token: {ex.Message}", null);
        }
    }

    public static (string status, string message, string? token) GenerateAccessToken(string refreshToken)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("JWT helper is not initialized. Call Initialize() first.");
        }

        try
        {
            var claims = GetTokenClaims(refreshToken);
            if (claims == null)
            {
                return ("ERROR", "Invalid refresh token", null);
            }

            // Verify it's a refresh token
            if (!claims.TryGetValue("token_type", out var tokenType) || tokenType != "refresh")
            {
                return ("ERROR", "Invalid token type", null);
            }

            // Get token target
            if (!claims.TryGetValue("token_target", out var targetStr) ||
                !Enum.TryParse<TokenTarget>(targetStr, out var target))
            {
                return ("ERROR", "Invalid token target", null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

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
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return ("SUCCESS", "Access token generated successfully", tokenHandler.WriteToken(token));
        }
        catch (Exception ex)
        {
            return ("ERROR", $"Failed to generate access token: {ex.Message}", null);
        }
    }

    public static bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("JWT helper is not initialized. Call Initialize() first.");
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
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

    private static IDictionary<string, string>? GetTokenClaims(string token)
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