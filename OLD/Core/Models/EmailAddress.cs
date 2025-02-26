using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;
using API.Core.Enums;
namespace API.Core.Models;

public class EmailAddress
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [Column("email")]
    public required string Email { get; set; }

    [Required]
    [Column("type")]
    public required EmailType Type { get; set; }

    [Required]
    [Column("state")]
    public required EmailState State { get; set; } = EmailState.Created;

    [Required]
    public required User User { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}