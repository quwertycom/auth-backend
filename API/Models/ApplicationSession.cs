using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class ApplicationSession
{
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
    public required ApplicationAccount ApplicationAccount { get; set; }

    [Required]
    public required long ApplicationAccountId { get; set; }

    [Required]
    public required Application Application { get; set; }

    [Required]
    public required long ApplicationId { get; set; }

    [Required]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}