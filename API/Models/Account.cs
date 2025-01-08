using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class Account {
    [Key]
    public long Id { get; set; }

    [Required]
    public required User User { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    public required string AccountName { get; set; }

    [Required]
    public required AccountType Type { get; set; }

    [Required]
    public bool IsPersonal { get; set; }

    public Organization? Organization { get; set; }

    public long? OrganizationId { get; set; }

    public virtual ICollection<OrganizationRole>? Roles { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}