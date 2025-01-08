using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

[Index(nameof(CreatedAt), Name = "IX_AuditLog_CreatedAt")]
public class AuditLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string EntityName { get; set; }

    [Required]
    public required long EntityId { get; set; }

    [Required]
    public required AuditAction Action { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public required long UserId { get; set; }

    [Required]
    [MaxLength(2048)]
    public required string Changes { get; set; }  // JSON of changes

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 