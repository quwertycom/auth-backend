using System.ComponentModel.DataAnnotations;
using API.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;
using System.Text.Json.Serialization;

namespace API.Core.Models;

public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; } = Snowflake.Generate();

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public required string Username { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public required string LastName { get; set; }

    [Required]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [Required]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [Required]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [Required]
    public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();

    [JsonIgnore]
    [Required]
    public virtual ICollection<EmailAddress> EmailAddresses { get; set; } = new List<EmailAddress>();

    [Required]
    [MaxLength(256)]
    [Column("password_hash")]
    public required string PasswordHash { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("password_salt")]
    public required string PasswordSalt { get; set; }

    [Required]
    [Column("birth_date")]
    public required DateTime BirthDate { get; set; }

    [Required]
    [Column("gender")]
    public UserGender Gender { get; set; }
    [Required]
    [Column("state")]
    public UserState State { get; set; } = UserState.PendingVerification;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }
}