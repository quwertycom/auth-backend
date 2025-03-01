using API.Shared.Utilities;
using API.Shared.Enums.Entities.User;

namespace API.Infrastructure.Database.Entities.User;

public class PhoneNumber
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Value { get; set; }
    public long UserId { get; set; }

    public required PhoneType Type { get; set; }
    public required PhoneState State { get; set; } = PhoneState.PendingVerification;

    public required User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}