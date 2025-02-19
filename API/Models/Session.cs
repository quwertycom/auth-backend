using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Enums;
using API.Common.Helpers;

namespace API.Models;

public class Session
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [Column("target")]
    public required SessionTarget Target { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    public Account? Account { get; set; }

    [Column("account_id")]
    public long? AccountId { get; set; }

    public Application? Application { get; set; }

    [Column("application_id")]
    public long? ApplicationId { get; set; }

    [Required]
    [Column("is_revoked")]
    public bool IsRevoked { get; set; } = false;

    [Required]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_used_at")]
    public DateTime? LastUsedAt { get; set; }

    [Column("application_account_id")]
    public long? ApplicationAccountId { get; set; }
}