using API.Shared.Utilities;

namespace API.Infrastructure.Database.Entities.Organization;

public class OrganizationRole
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Name { get; set; }
    public required string Description { get; set; }
    public required long OrganizationId { get; set; }

    public required virtual Organization Organization { get; set; }
    
    public virtual ICollection<Account.Account> Members { get; set; } = new List<Account.Account>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}