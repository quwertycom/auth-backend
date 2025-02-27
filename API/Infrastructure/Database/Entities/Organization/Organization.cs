using API.Shared.Utilities;

namespace API.Infrastructure.Database.Entities.Organization;

public class Organization
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public required string Description { get; set; }
    
    public virtual ICollection<Account.Account> Members { get; set; } = new List<Account.Account>();
    public virtual ICollection<OrganizationRole> Roles { get; set; } = new List<OrganizationRole>();
    public virtual ICollection<Developer.Developer> Developers { get; set; } = new List<Developer.Developer>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}