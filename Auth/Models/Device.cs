using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Models;

public class Device
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required DateTime LastUsed { get; set; } = DateTime.UtcNow;
    public required string Platform { get; set; }

    public required User User { get; set; }

    public required List<string> Fingerprint { get; set; } = new();
    public required List<AccountSession> AccountSessions { get; set; } = new();
    public required List<UserSession> UserSessions { get; set; } = new();
    public required List<ApplicationSession> ApplicationSessions { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Models.User))]
    public required string UserId { get; set; }
}