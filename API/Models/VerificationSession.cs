using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class VerificationSession
{
    [Key]
    [Column("id")]
    public required long Id { get; set; }

    [Required]
    [Column("email")]
    public required UserEmail Email { get; set; }

    [Required]
    [Column("email_id")]
    public long EmailId { get; set; }

    [Required]
    [Column("code")]
    public required string Code { get; set; }

    [Required]
    [Column("is_used")]
    public required bool IsUsed { get; set; } = false;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("expiry_minutes")]
    public int ExpiryMinutes { get; set; } = 15;
}