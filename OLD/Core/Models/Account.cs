using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Core.Enums;
using Microsoft.EntityFrameworkCore;
using API.Common.Helpers;

namespace API.Core.Models;

[Index(nameof(UserId), nameof(IsPersonal), IsUnique = true, Name = "IX_OnePersonalAccountPerUser")]
public class Account
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    public required virtual User User { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("account_name")]
    public required string AccountName { get; set; }

    [Required]
    [Column("type")]
    public required AccountType Type { get; set; }

    [Required]
    [Column("is_personal")]
    public bool IsPersonal { get; set; }

    public virtual Organization? Organization { get; set; }

    [Column("organization_id")]
    public long? OrganizationId { get; set; }

    public virtual ICollection<OrganizationRole> Roles { get; set; } = new List<OrganizationRole>();

    [Required]
    public virtual ICollection<Developer> AuthorizedDevelopers { get; set; } = new List<Developer>();

    [Required]
    public virtual ICollection<ApplicationAccount> AuthorizedApplications { get; set; } = new List<ApplicationAccount>();

    [Required]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}