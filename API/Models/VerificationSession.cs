using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class VerificationSession
{
    [Key]
    public required long Id { get; set; }
    [Required]
    public required UserEmail Email { get; set; }
    [Required]
    public long EmailId { get; set; }
    [Required]
    public required string Code { get; set; }
    [Required]
    public required bool IsUsed { get; set; } = false;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public int ExpiryMinutes { get; set; } = 15;
}