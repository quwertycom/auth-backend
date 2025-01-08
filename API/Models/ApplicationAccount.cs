using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class ApplicationAccount
{
    [Key]
    public long Id { get; set; }

    [Required]
    public required Application Application { get; set; }

    [Required]
    public required long ApplicationId { get; set; }

    [Required]
    public required Account Account { get; set; }

    [Required]
    public required long AccountId { get; set; }

    [Required]
    public required virtual ICollection<ApplicationSession> Sessions { get; set; } = new List<ApplicationSession>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}