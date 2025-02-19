using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Enums;
using API.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

[Index(nameof(Type), IsUnique = true, Name = "IX_OnePersonalDeveloperPerAccount")]
public class Developer
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [StringLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [Column("status")]
    public required DeveloperStatus Status { get; set; }

    [Required]
    [Column("type")]
    public required DeveloperType Type { get; set; }

    // Organization is required when Type is Organization
    public virtual Organization? Organization { get; set; }

    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    // Collection of authorized accounts that can access this developer profile
    [Required]
    public virtual ICollection<Account> AuthorizedAccounts { get; set; } = new List<Account>();

    [Required]
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}