using System.ComponentModel.DataAnnotations;

public class UserEmail {
    [Key]
    public long Id { get; set; }

    [Required]
    [EmailAddress] 
    public required string Email { get; set; }

    [Required]
    public EmailState State { get; set; } = EmailState.Unverified;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public required User User { get; set; }

    [Required]
    public long UserId { get; set; }
}