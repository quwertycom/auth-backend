using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class Organization {
    [Key]
    public long Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public virtual ICollection<Account> Members { get; set; } = new List<Account>();

    [Required]
    public virtual ICollection<OrganizationRole> Roles { get; set; } = new List<OrganizationRole>();

    public virtual ICollection<Developer> DeveloperProfiles { get; set; } = new List<Developer>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}