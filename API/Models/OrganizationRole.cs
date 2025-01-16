using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;

namespace API.Models;

public class OrganizationRole
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
    public required virtual Organization Organization { get; set; }

    [Required]
    [Column("organization_id")]
    public required long OrganizationId { get; set; }

    [Required]
    public virtual ICollection<Account> Members { get; set; } = new List<Account>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}