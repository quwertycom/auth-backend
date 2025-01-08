using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
namespace API.Models;

public class Token {
    [Key]
    public long Id { get; set; }

    [Required]
    public required string TokenString { get; set; }

    [Required]
    public required TokenType Type { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public required long UserId { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; } = null;

    [Required]
    public required UserSession UserSession { get; set; }

    [Required]
    public required long UserSessionId { get; set; }
}