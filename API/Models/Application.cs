using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class Application {
    [Key]
    public long Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required string IconUrl { get; set; }

    [Required]
    public required string RedirectUri { get; set; }

    [Required]
    public required ApplicationStatus Status { get; set; } = ApplicationStatus.Development;

    [Required]
    public required virtual Developer Developer { get; set; }

    [Required]
    public required long DeveloperId { get; set; }

    [Required]
    public required virtual ICollection<ApplicationAccount> Accounts { get; set; } = new List<ApplicationAccount>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}