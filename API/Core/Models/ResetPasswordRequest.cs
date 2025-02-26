using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Common.Helpers;

namespace API.Core.Models
{
    public class ResetPasswordRequest
    {
        [Key]
        [Column("id")]
        public long Id { get; set; } = Snowflake.Generate();

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [Required]
        public required User User { get; set; }

        [Required]
        [Column("email_id")]
        public long EmailId { get; set; }

        [Required]
        public required EmailAddress EmailAddress { get; set; }

        [Required]
        [Column("code_hash")]
        public required string CodeHash { get; set; }

        [Required]
        [Column("is_used")]
        public required bool IsUsed { get; set; } = false;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("expired_at")]
        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    }
}