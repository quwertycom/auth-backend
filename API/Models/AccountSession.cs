using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class AccountSession {
    [Key]
    public long Id { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public required long UserId { get; set; }

    [Required]
    public required Account Account { get; set; }

    [Required]
    public required long AccountId { get; set; }

    [Required]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }
}