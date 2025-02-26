using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;

namespace API.Core.Models;

public class Organization
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [StringLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [StringLength(500)]
    [Column("description")]
    public required string Description { get; set; }

    [Required]
    [Column("members")]
    public virtual ICollection<Account> Members { get; set; } = new List<Account>();

    [Required]
    [Column("roles")]
    public virtual ICollection<OrganizationRole> Roles { get; set; } = new List<OrganizationRole>();

    [Column("developers")]
    public virtual ICollection<Developer> Developers { get; set; } = new List<Developer>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}