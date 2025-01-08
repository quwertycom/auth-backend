using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
namespace API.Models;

public class Developer {
    [Key]
    public long Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required DeveloperStatus Status { get; set; }

    [Required]
    public required DeveloperType Type { get; set; }

    public Organization? Organization { get; set; }

    public long? OrganizationId { get; set; }

    public virtual ICollection<Account> AuthorizedAccounts { get; set; } = new List<Account>();
    
    [Required]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}