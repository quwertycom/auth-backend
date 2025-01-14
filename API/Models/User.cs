using System.ComponentModel.DataAnnotations;
using API.Common.Enums;

namespace API.Models;

public class User
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Username { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    public virtual ICollection<UserEmail> Emails { get; set; } = new List<UserEmail>();

    [Required]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [Required]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [Required]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; } = null;

    [Required]
    [MaxLength(256)]
    public required string PasswordHash { get; set; }

    [Required]
    [MaxLength(128)]
    public required string PasswordSalt { get; set; }

    [Required]
    public UserState State { get; set; } = UserState.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}