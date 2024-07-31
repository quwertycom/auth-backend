using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Auth.Helpers
{
    public static class JWT
    {
        private static readonly string Issuer;
        private static readonly string Audience;
        private static readonly string Key;

        static JWT()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Issuer = configuration["JWT:Issuer"] ?? throw new ArgumentException("JWT:Issuer is not set in the configuration.");
            Audience = configuration["JWT:Audience"] ?? throw new ArgumentException("JWT:Audience is not set in the configuration.");
            Key = configuration["JWT:Key"] ?? throw new ArgumentException("JWT:Key is not set in the configuration.");
        }

        public static (bool isValid, string? type) VerifyToken(string token, string audience)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var key = Encoding.ASCII.GetBytes(Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                var validationResult = tokenHandler.ValidateToken(token, validationParameters);
                if (validationResult.IsValid)
                {
                    var expirationClaim = validationResult.ClaimsIdentity?.FindFirst(JwtRegisteredClaimNames.Exp);
                    if (expirationClaim != null)
                    {
                        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationClaim.Value))
                            .UtcDateTime;
                        var tokenType = expirationTime <= DateTime.UtcNow.AddDays(7) ? "access" : "refresh";
                        return (true, tokenType);
                    }
                }

                return (false, null);
            }
            catch
            {
                return (false, null);
            }
        }

        public static string GetUserIdFromToken(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var key = Encoding.ASCII.GetBytes(Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                var validationResult = tokenHandler.ValidateToken(token, validationParameters);
                if (validationResult.IsValid)
                {
                    var userIdClaim = validationResult.ClaimsIdentity?.FindFirst(JwtRegisteredClaimNames.Sub);
                    return userIdClaim?.Value;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static class User
        {
            public static string GenerateAccessToken(string userId, string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userId),
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }

            public static string GenerateRefreshToken(string userId, string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userId),
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }

            public static string GenerateServiceToken(string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }
        }

        public static class Account
        {
            public static string GenerateAccessToken(string userId, string accountId, string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userId),
                        new Claim("AccountId", accountId),
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }

            public static string GenerateRefreshToken(string userId, string accountId, string sessionId,
                string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userId),
                        new Claim("AccountId", accountId),
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }

            public static string GenerateServiceToken(string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }
        }

        public static class Application
        {
            public static string GenerateAccessToken(string userId, string accountId, string applicationId,
                string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userId),
                        new Claim("AccountId", accountId),
                        new Claim("ApplicationId", applicationId),
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }

            public static string GenerateRefreshToken(string userId, string accountId, string applicationId,
                string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userId),
                        new Claim("AccountId", accountId),
                        new Claim("ApplicationId", applicationId),
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                return tokenHandler.CreateToken(tokenDescriptor);
            }

            public static string GenerateServiceToken(string sessionId, string audience)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var key = Encoding.ASCII.GetBytes(Key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("SessionId", sessionId),
                        new Claim("JTI", Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    Issuer = Issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                return tokenHandler.CreateToken(tokenDescriptor);
            }
        }
    }
}