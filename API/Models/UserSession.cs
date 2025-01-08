using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class UserSession
{
    [Key]
    public long Id { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }
}