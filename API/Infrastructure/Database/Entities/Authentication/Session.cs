using API.Shared.Utilities;
using API.Shared.Enums.Authentication;

namespace API.Infrastructure.Database.Entities.Authentication;

public class Session
{
    public long Id { get; set; } = Snowflake.Generate();

    public bool IsRevoked { get; set; } = false;
    public long UserId { get; set; }
    public long? AccountId { get; set; }
    public long? ApplicationId { get; set; }
    public long? ApplicationAccountId { get; set; }

    public required SessionTarget Target { get; set; }

    public required User.User User { get; set; }
    public Account.Account? Account { get; set; }
    public Application.Application? Application { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}