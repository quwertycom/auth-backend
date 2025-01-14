using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Enums;
namespace API.Models;

public class Token
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [StringLength(512)]
    [Column("token_string")]
    public required string TokenString { get; set; }

    [Required]
    [Column("type")]
    public required TokenType Type { get; set; }

    [Required]
    [Column("target")]
    public required TokenTarget Target { get; set; }

    [Required]
    public required Session Session { get; set; }

    [Required]
    [Column("session_id")]
    public required long SessionId { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    [Column("user_id")]
    public required long UserId { get; set; }

    public Account? Account { get; set; }

    [Column("account_id")]
    public long? AccountId { get; set; }

    public ApplicationAccount? ApplicationAccount { get; set; }

    [Column("application_account_id")]
    public long? ApplicationAccountId { get; set; }

    public Application? Application { get; set; }

    [Column("application_id")]
    public long? ApplicationId { get; set; }

    [Required]
    [Column("created_at")]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; } = null;
}