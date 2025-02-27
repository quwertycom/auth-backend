using API.Shared.Utilities;

namespace API.Infrastructure.Database.Entities.Application;

public class ApplicationAccount
{
    public long Id { get; set; } = Snowflake.Generate();

    public required long ApplicationId { get; set; }
    public required long AccountId { get; set; }

    public required Application Application { get; set; }
    public required Account.Account Account { get; set; }

    public required virtual ICollection<Authentication.Session> Sessions { get; set; } = new List<Authentication.Session>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}