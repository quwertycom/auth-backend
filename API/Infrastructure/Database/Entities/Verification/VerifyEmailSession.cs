using API.Shared.Utilities;
using API.Infrastructure.Database.Entities.User;

namespace API.Infrastructure.Database.Entities.Verification;

public class VerifyEmailSession
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Code { get; set; }
    public required bool IsUsed { get; set; } = false;
    public long UserId { get; set; }
    public long EmailId { get; set; }

    public required User.User User { get; set; }
    public required EmailAddress Email { get; set; }

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}