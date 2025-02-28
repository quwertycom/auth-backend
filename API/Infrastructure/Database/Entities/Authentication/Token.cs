using API.Shared.Utilities;
using API.Shared.Enums.Entities.Authentication;

namespace API.Infrastructure.Database.Entities.Authentication;

public class Token
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Value { get; set; }
    public bool IsRefreshed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public long SessionId { get; set; }
    public long UserId { get; set; }
    public long? AccountId { get; set; }
    public long? ApplicationAccountId { get; set; }
    public long? ApplicationId { get; set; }
    public long? ParentTokenId { get; set; }

    public required TokenType Type { get; set; }
    public required TokenTarget Target { get; set; }

    public required Session Session { get; set; }
    public required User.User User { get; set; }
    public Account.Account? Account { get; set; }
    public Application.Application? Application { get; set; }
    public Application.ApplicationAccount? ApplicationAccount { get; set; }
    public Token? ParentToken { get; set; }
 
    public DateTime? ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}