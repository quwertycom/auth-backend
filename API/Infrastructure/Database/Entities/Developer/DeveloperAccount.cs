using API.Shared.Utilities;
using API.Shared.Enums.Entities.Developer;

namespace API.Infrastructure.Database.Entities.Developer;

public class DeveloperAccount
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public required long DeveloperId { get; set; }
    public required long AccountId { get; set; }

    public required DeveloperStatus Status { get; set; }
    public required DeveloperType Type { get; set; }

    public required virtual Developer Developer { get; set; }
    public required virtual Account.Account Account { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}