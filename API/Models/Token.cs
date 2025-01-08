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
    public required User User { get; set; }

    [Required]
    public required long UserId { get; set; }

    // Optional relationships based on TokenTarget
    public ApplicationAccount? ApplicationAccount { get; set; }
    public long? ApplicationAccountId { get; set; }

    public Account? Account { get; set; }
    public long? AccountId { get; set; }

    public UserSession? UserSession { get; set; }
    public long? UserSessionId { get; set; }

    public AccountSession? AccountSession { get; set; }
    public long? AccountSessionId { get; set; }

    public ApplicationSession? ApplicationSession { get; set; }
    public long? ApplicationSessionId { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; } = null;
}