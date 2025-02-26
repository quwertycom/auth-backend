using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;
using API.Core.Enums;
namespace API.Core.Models;

public class PhoneNumber
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [Column("phone")]
    public required string Phone { get; set; }

    [Required]
    [Column("type")]
    public required PhoneType Type { get; set; }

    [Required]
    [Column("state")]
    public required PhoneState State { get; set; } = PhoneState.Created;

    [Required]
    public required User User { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}