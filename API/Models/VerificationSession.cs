using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;

namespace API.Models;

public class VerificationSession
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    public EmailAddress? Email { get; set; }

    [Column("email_id")]
    public long? EmailId { get; set; }

    public PhoneNumber? Phone { get; set; }

    [Column("phone_id")]
    public long? PhoneId { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

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