using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class UserEmail
{
    [Key]
    [Column("user_email_id")]
    public long UserEmailId { get; set; }

    [Required]
    [EmailAddress]
    [Column("email")]
    public required string Email { get; set; }

    [Required]
    [Column("state")]
    public EmailState State { get; set; } = EmailState.Unverified;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public required User User { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }
}