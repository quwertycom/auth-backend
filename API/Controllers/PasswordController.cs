using API.Models;
using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[ApiController]
	[Route("api/password")]
	[Authorize]
	public class PasswordController : Controller
	{
		private readonly IPasswordService _passwordService;
		public PasswordController(IPasswordService passwordService)
		{
			_passwordService = passwordService;
		}
		[HttpPost("sent-otp")]
		public async Task SendOTP([FromBody] long UserId)
		{
			await _passwordService.SendOTP(UserId);
		}
		[HttpPost("reset-password")]
		public async Task ResetPassword(ResetPasswordRequest model)
		{
			await _passwordService.ChangePassword(model.UserId, model.Password, model.OTP);
		}
	}
}
