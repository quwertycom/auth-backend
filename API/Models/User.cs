using System.ComponentModel.DataAnnotations;

public class User {
    [Key]
    public long Id { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string FirstName { get; set; }
    
    [Required] 
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    [Required]
    public required string PasswordSalt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}