using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class User
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public required string Username { get; set; }

    [Required]
    [StringLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public required string LastName { get; set; }

    [Required]
    public virtual ICollection<UserEmail> Emails { get; set; } = new List<UserEmail>();

    [Required]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; } = null;

    [Required]
    [StringLength(256)]
    public required string PasswordHash { get; set; }

    [Required]
    [StringLength(128)]
    public required string PasswordSalt { get; set; }

    [Required]
    public UserState State { get; set; } = UserState.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}