using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

[Index(nameof(Type), IsUnique = true, Name = "IX_OnePersonalDeveloperPerAccount")]
public class Developer {
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    public required DeveloperStatus Status { get; set; }

    [Required]
    public required DeveloperType Type { get; set; }

    // Organization is required when Type is Organization
    public virtual Organization? Organization { get; set; }
    public long? OrganizationId { get; set; }

    // Collection of authorized accounts that can access this developer profile
    [Required]
    public virtual ICollection<Account> AuthorizedAccounts { get; set; } = new List<Account>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}