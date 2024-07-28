namespace Auth.Models;

public class AccountSession
{
    public required string Id { get; set; }
    public required string RefreshToken { get; set; }
    public required string AccessToken { get; set; }
    public required string DeviceFingerprint { get; set; }

    public required Account Account { get; set; }

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}