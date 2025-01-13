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
            if (target == TokenTarget.User)
            {
                if (string.IsNullOrEmpty(ids.userId.ToString()))
                {
                    return ("ERROR", "Missing user id", null);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                // Generate a random value for jti (JWT ID)
                var randomBytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var jti = Convert.ToBase64String(randomBytes);

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, ids.userId.ToString()),
                    new(JwtRegisteredClaimNames.Jti, jti),
                    new("token_type", "refresh"),
                    new("token_target", TokenTarget.User.ToString())
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(30), // Refresh tokens typically have a longer lifetime
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
            else if (target == TokenTarget.Account)
            {
                if (!ids.accountId.HasValue || string.IsNullOrEmpty(ids.userId.ToString()) || string.IsNullOrEmpty(ids.accountId.ToString()))
                {
                    return ("ERROR", "Missing account id", null);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                // Generate a random value for jti (JWT ID)
                var randomBytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var jti = Convert.ToBase64String(randomBytes);

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, ids.accountId.Value.ToString()),
                    new(JwtRegisteredClaimNames.Jti, jti),
                    new("user_id", ids.userId.ToString()),
                    new("token_type", "refresh"),
                    new("token_target", TokenTarget.Account.ToString())
                };

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
            else if (target == TokenTarget.Application)
            {
                if (!ids.applicationId.HasValue || !ids.accountId.HasValue || string.IsNullOrEmpty(ids.userId.ToString()) || string.IsNullOrEmpty(ids.accountId.ToString()) || string.IsNullOrEmpty(ids.applicationId.ToString()))
                {
                    return ("ERROR", "Missing application id or account id", null);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                // Generate a random value for jti (JWT ID)
                var randomBytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var jti = Convert.ToBase64String(randomBytes);

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, ids.applicationId.Value.ToString()),
                    new(JwtRegisteredClaimNames.Jti, jti),
                    new("user_id", ids.userId.ToString()),
                    new("account_id", ids.accountId.Value.ToString()),
                    new("token_type", "refresh"),
                    new("token_target", TokenTarget.Application.ToString())
                };

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
            else
            {
                return ("ERROR", "Something went wrong", null);
            }
        }
        catch
        {
            return ("ERROR", "Internal Error", null);
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
}