using API.Shared.Utilities;
using API.Shared.Enums.Entities.User;

namespace API.Infrastructure.Database.Entities.User;

public class EmailAddress
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Value { get; set; }
    public long UserId { get; set; }

    public required EmailType Type { get; set; }
    public required EmailState State { get; set; } = EmailState.Created;

    public required User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}