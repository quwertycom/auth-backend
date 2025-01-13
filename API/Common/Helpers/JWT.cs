using API.Common.Enums;

namespace API.Common.Helpers;

public static class JWT
{
    public static (string status, string? token) GenerateRefreshToken(TokenTarget target, (long userId, long? accountId, long? applicationId) ids)
    {
        try
        {
            return ("SUCCESS", "token");
        }
        catch
        {
            return ("ERROR", null);
        }
    }
}