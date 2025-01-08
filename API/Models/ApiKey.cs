using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class ApiKey
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(512)]
    public required string KeyHash { get; set; }

    [Required]
    [StringLength(128)]
    public required string KeySalt { get; set; }

    [Required]
    public required Application Application { get; set; }

    [Required]
    public required long ApplicationId { get; set; }

    [Required]
    public ApiKeyStatus Status { get; set; } = ApiKeyStatus.Active;

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }
} 