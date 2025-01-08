using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class Application
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    [Required]
    [StringLength(2048)]
    [Url]
    public required string IconUrl { get; set; }

    [Required]
    [StringLength(2048)]
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}