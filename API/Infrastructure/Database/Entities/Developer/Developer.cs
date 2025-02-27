using API.Shared.Utilities;
using API.Shared.Enums.Developer;

namespace API.Infrastructure.Database.Entities.Developer;

public class Developer
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public required string ContactEmail { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }

    public required DeveloperType Type { get; set; }
    public required DeveloperStatus Status { get; set; }

    public virtual ICollection<DeveloperAccount> Accounts { get; set; } = new List<DeveloperAccount>();
    public virtual ICollection<Application.Application> Applications { get; set; } = new List<Application.Application>();

    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}