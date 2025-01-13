using System.ComponentModel.DataAnnotations;
using API.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

[Index(nameof(UserId), nameof(IsPersonal), IsUnique = true, Name = "IX_OnePersonalAccountPerUser")]
public class Account
{
    [Key]
    public long Id { get; set; }

    [Required]
    public required virtual User User { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    [StringLength(100)]
    public required string AccountName { get; set; }

    [Required]
    public required AccountType Type { get; set; }

    [Required]
    public bool IsPersonal { get; set; }

    public virtual Organization? Organization { get; set; }

    public long? OrganizationId { get; set; }

    public virtual ICollection<OrganizationRole> Roles { get; set; } = new List<OrganizationRole>();

    [Required]
    public virtual ICollection<Developer> AuthorizedDevelopers { get; set; } = new List<Developer>();

    [Required]
    public virtual ICollection<ApplicationAccount> AuthorizedApplications { get; set; } = new List<ApplicationAccount>();

    [Required]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}