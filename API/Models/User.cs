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
    public virtual ICollection<UserEmail> Emails { get; set; } = new List<UserEmail>();
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    [Required]
    public required string PasswordSalt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
}