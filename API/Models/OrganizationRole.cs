using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class OrganizationRole
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
    public required virtual Organization Organization { get; set; }

    [Required]
    public required long OrganizationId { get; set; }

    [Required]
    public virtual ICollection<Account> Members { get; set; } = new List<Account>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}