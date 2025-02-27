using API.Shared.Utilities;

namespace API.Infrastructure.Database.Entities.Verification;

public class ResetPasswordRequest
{
    public long Id { get; set; } = Snowflake.Generate();
    
    public required string CodeHash { get; set; }
    public required bool IsUsed { get; set; } = false;
    public long UserId { get; set; }
    public long EmailId { get; set; }

    public required User.User User { get; set; }
    public required User.EmailAddress EmailAddress { get; set; }

    public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}