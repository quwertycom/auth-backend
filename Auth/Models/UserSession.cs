namespace Auth.Models;

public class UserSession
{
    public required string Id { get; set; }
    public required string RefreshToken { get; set; }
    public required string AccessToken { get; set; }
    public required string DeviceFingerprint { get; set; }
    public required bool IsRevoked { get; set; }

    public required User User { get; set; }
    public required Device Device { get; set; }

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public required string UserId { get; set; }
    public required string DeviceId { get; set; }
}