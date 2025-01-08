using System.ComponentModel.DataAnnotations;

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

    public Organization? Organization { get; set; } = null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}