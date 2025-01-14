using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
namespace API.Models;

public class Token
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(512)]
    public required string TokenString { get; set; }

    [Required]
    public required TokenType Type { get; set; }

    [Required]
    public required TokenTarget Target { get; set; }

    [Required]
    public required Session Session { get; set; }

    [Required]
    public required long SessionId { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public required long UserId { get; set; }

    public Account? Account { get; set; }

    public long? AccountId { get; set; }

    public ApplicationAccount? ApplicationAccount { get; set; }

    public long? ApplicationAccountId { get; set; }

    public Application? Application { get; set; }

    public long? ApplicationId { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; } = null;
}