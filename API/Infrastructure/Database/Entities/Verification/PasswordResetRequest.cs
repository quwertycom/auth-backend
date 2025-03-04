using API.Shared.Utilities;

namespace API.Infrastructure.Database.Entities.Verification;

public class PasswordResetRequest
{
    public long Id { get; set; } = Snowflake.Generate();
    
    public required string CodeHash { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public long UserId { get; set; }
    public long EmailId { get; set; }

    public required User.User User { get; set; }
    public required User.EmailAddress EmailAddress { get; set; }

    public DateTime? UsedAt { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}