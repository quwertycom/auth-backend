using Microsoft.AspNetCore.Mvc;

namespace API.Models
{
	public class ResetPasswordModel
	{
		public long UserId { get; set; }
		public string Password { get; set; }
		public string OTP { get; set; }
	}
}
