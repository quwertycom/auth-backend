using API.Shared.Utilities;
using API.Shared.Enums.Application;

namespace API.Infrastructure.Database.Entities.Application;

public class Application
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string IconUrl { get; set; }
    public required string RedirectUri { get; set; }
    public required long DeveloperId { get; set; }

    public required ApplicationStatus Status { get; set; } = ApplicationStatus.Development;

    public required virtual Developer.DeveloperAccount Developer { get; set; }

    public virtual ICollection<ApplicationAccount> Accounts { get; set; } = new List<ApplicationAccount>();
    public virtual ICollection<Authentication.Session> Sessions { get; set; } = new List<Authentication.Session>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}