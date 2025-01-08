using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

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
    
    [Required]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    
    [Phone]
    public string? PhoneNumber { get; set; } = null;
    
    [Required]
    public required string PasswordHash { get; set; }
    
    [Required]
    public required string PasswordSalt { get; set; }

    [Required]
    public UserState State { get; set; } = UserState.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
}