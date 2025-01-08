using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class Application
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Description { get; set; }

    [Required]
    [MaxLength(2048)]
    [Url]
    public required string IconUrl { get; set; }

    [Required]
    [MaxLength(2048)]
    [Url]
    public required string RedirectUri { get; set; }

    [Required]
    public required ApplicationStatus Status { get; set; } = ApplicationStatus.Development;

    [Required]
    public virtual Developer Developer { get; set; }

    [Required]
    public required long DeveloperId { get; set; }

    [Required]
    public virtual ICollection<ApplicationAccount> Accounts { get; set; } = new List<ApplicationAccount>();

    [Required]
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

    [Required]
    public virtual ICollection<ApplicationSession> Sessions { get; set; } = new List<ApplicationSession>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}