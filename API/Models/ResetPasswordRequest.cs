using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models
{
	public class ResetPasswordRequest
	{
		[Required]
		[Column("user_id")]
		public required long UserId { get; set; }
		
		[Required]
		[Column("password")]
		public required string Password { get; set; }
		
		[Required]
		[Column("otp")]
		public required string OTP { get; set; }
	}
}
