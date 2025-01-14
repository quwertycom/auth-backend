using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class ApplicationAccount
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    public required Application Application { get; set; }

    [Required]
    [Column("application_id")]
    public required long ApplicationId { get; set; }

    [Required]
    public required Account Account { get; set; }

    [Required]
    [Column("account_id")]
    public required long AccountId { get; set; }

    [Required]
    public required virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}