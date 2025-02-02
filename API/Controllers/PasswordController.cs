using API.Models;
using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[ApiController]
	[Route("Password")]
	[Authorize]
	public class PasswordController : Controller
	{
		private readonly IPasswordService _passwordService;
		public PasswordController(IPasswordService passwordService)
		{
			_passwordService = passwordService;
		}
		[HttpPost("SentOTP")]
		public async Task SendOTP([FromBody] long UserId)
		{
			await _passwordService.SendOTP(UserId);
		}
		[HttpPost("ResetPassword")]
		public async Task ResetPassword(ResetPasswordModel model)
		{
			await _passwordService.ChangePassword(model.UserId, model.Password, model.OTP);
		}
	}
}
