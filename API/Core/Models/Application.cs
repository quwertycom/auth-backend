using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Core.Enums;
using API.Common.Helpers;

namespace API.Core.Models;

public class Application
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("description")]
    public required string Description { get; set; }

    [Required]
    [MaxLength(2048)]
    [Url]
    [Column("icon_url")]
    public required string IconUrl { get; set; }

    [Required]
    [MaxLength(2048)]
    [Url]
    [Column("redirect_uri")]
    public required string RedirectUri { get; set; }

    [Required]
    [Column("status")]
    public required ApplicationStatus Status { get; set; } = ApplicationStatus.Development;

    [Required]
    public required virtual Developer Developer { get; set; }

    [Required]
    [Column("developer_id")]
    public required long DeveloperId { get; set; }

    [Required]
    public virtual ICollection<ApplicationAccount> Accounts { get; set; } = new List<ApplicationAccount>();

    [Required]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}