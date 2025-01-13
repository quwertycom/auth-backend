using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Enums;

namespace API.Models;

public class Session
{
    [Key]
    public long Id { get; set; }

    [Required]
    public required SessionTarget Target { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public required long UserId { get; set; }

    [Required]
    public required User User { get; set; }

    [ForeignKey(nameof(Account))]
    public long? AccountId { get; set; }

    public Account? Account { get; set; }

    [ForeignKey(nameof(Application))]
    public long? ApplicationId { get; set; }

    public Application? Application { get; set; }

    [Required]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }
}