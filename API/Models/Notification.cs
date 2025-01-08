using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class Notification
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public required string Message { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public required long UserId { get; set; }

    public Account? Account { get; set; }
    public long? AccountId { get; set; }

    public Application? Application { get; set; }
    public long? ApplicationId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }
}