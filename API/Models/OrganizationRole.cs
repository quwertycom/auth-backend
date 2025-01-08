using System.ComponentModel.DataAnnotations;

public class OrganizationRole {
    [Key]
    public long Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required Organization Organization { get; set; }

    [Required]
    public required long OrganizationId { get; set; }

    [Required]
    public required ICollection<Account> Members { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}