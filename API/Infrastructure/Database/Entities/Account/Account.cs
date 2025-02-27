using API.Shared.Utilities;
using API.Shared.Enums.Account;

namespace API.Infrastructure.Database.Entities.Account;

public class Account
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public bool IsPersonal { get; set; }
    public long UserId { get; set; }
    public long? OrganizationId { get; set; }

    public required AccountType Type { get; set; }

    public virtual Organization.Organization? Organization { get; set; }
    public virtual required User.User User { get; set; }

    public virtual ICollection<Application.ApplicationAccount> AuthorizedApplications { get; set; } = new List<Application.ApplicationAccount>();
    public virtual ICollection<Developer.DeveloperAccount> DeveloperAccounts { get; set; } = new List<Developer.DeveloperAccount>();
    public virtual ICollection<Authentication.Session> Sessions { get; set; } = new List<Authentication.Session>();
    public virtual ICollection<Organization.OrganizationRole>? Roles { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}