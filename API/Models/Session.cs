using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Enums;

namespace API.Models;

public class Session
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("target")]
    public required SessionTarget Target { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public required long UserId { get; set; }

    [Required]
    public required User User { get; set; }

    [ForeignKey(nameof(Account))]
    [Column("account_id")]
    public long? AccountId { get; set; }

    public Account? Account { get; set; }

    [ForeignKey(nameof(Application))]
    [Column("application_id")]
    public long? ApplicationId { get; set; }

    public Application? Application { get; set; }

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