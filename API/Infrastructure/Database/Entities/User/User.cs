using API.Shared.Enums.User;
using API.Shared.Utilities;

namespace API.Infrastructure.Database.Entities.User;

public class User
{
    public long Id { get; set; } = Snowflake.Generate();

    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }
    
    public required DateTime BirthDate { get; set; }
    public required UserGender Gender { get; set; }
    public required UserState State { get; set; } = UserState.PendingVerification;

    public virtual ICollection<Account.Account> Accounts { get; set; } = new List<Account.Account>();
    public virtual ICollection<Authentication.Session> Sessions { get; set; } = new List<Authentication.Session>();
    public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
    public virtual ICollection<EmailAddress> EmailAddresses { get; set; } = new List<EmailAddress>();

    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public byte[]? RowVersion { get; set; }
}