using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;

namespace API.Models;

public class UserEmail
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

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

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;
}