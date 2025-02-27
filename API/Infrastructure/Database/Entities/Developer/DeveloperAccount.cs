using API.Shared.Utilities;
using API.Shared.Enums.Developer;

namespace API.Infrastructure.Database.Entities.Developer;

public class DeveloperAccount
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public long? OrganizationId { get; set; }

    public required DeveloperStatus Status { get; set; }
    public required DeveloperType Type { get; set; }

    public virtual Organization.Organization? Organization { get; set; }

    public virtual ICollection<Account.Account> AuthorizedAccounts { get; set; } = new List<Account.Account>();
    public virtual ICollection<Application.Application> Applications { get; set; } = new List<Application.Application>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}