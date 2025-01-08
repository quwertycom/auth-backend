using System.ComponentModel.DataAnnotations;

public class Organization {
    [Key]
    public long Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required ICollection<Account> Members { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}