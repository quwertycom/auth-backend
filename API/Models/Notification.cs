using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Enums;
using API.Common.Helpers;

namespace API.Models;

public class Notification
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [StringLength(200)]
    [Column("title")]
    public required string Title { get; set; }

    [Required]
    [StringLength(1000)]
    [Column("message")]
    public required string Message { get; set; }

    [Required]
    [Column("type")]
    public NotificationType Type { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    [Column("user_id")]
    public required long UserId { get; set; }

    public Account? Account { get; set; }
    
    [Column("account_id")]
    public long? AccountId { get; set; }

    public Application? Application { get; set; }
    
    [Column("application_id")]
    public long? ApplicationId { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }
}