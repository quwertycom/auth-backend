using API.Common.Helpers;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace API.Service;
public interface IPasswordService
{
	public Task ChangePassword(long UsertId, string Password, string otp);
	public Task SendOTP(long UserId);
}
public class PasswordService : IPasswordService
{
	private readonly AuthDbContext _Context;
	public PasswordService(AuthDbContext context)
	{
		_Context = context;
	}

	public async Task ChangePassword(long UsertId, string Password, string otp)
	{
		var User = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UsertId);
		if (User != null && await CheckIsOTPValid(UsertId, otp))
		{
			var hashedPassword = PasswordHasher.Hash(Password);
			User.PasswordHash = hashedPassword.hash;
			User.PasswordSalt = hashedPassword.salt;
			await _Context.SaveChangesAsync();
		}
	}
	public async Task SendOTP(long UserId)
	{
		var Email = await _Context.UserEmails.FirstOrDefaultAsync(x => x.UserId == UserId);
		var User = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
		if (User != null && Email != null)
		{
			var otp = OTPGenerator.GenerateOTP();
			var otpSession = new VerificationSession
			{
				Email = Email,
				User = User,
				Code = otp,
				IsUsed = false,
			};
			_Context.VerificationSessions.Add(otpSession);
			if (await _Context.SaveChangesAsync() > 0)
			{
				await EmailSender.SendOtpEmailAsync(Email.Email, otp);
			}
		}
	}
	private async Task<bool> CheckIsOTPValid(long UserId, string OTP)
	{
		var UserOTP = await _Context.VerificationSessions.FirstOrDefaultAsync(x => x.UserId == UserId && x.CreatedAt.AddMinutes(x.ExpiryMinutes) > DateTime.Now);
		if (UserOTP != null)
		{
			if (UserOTP.Code.Equals(OTP))
			{
				return true;
			}
		}
		return false;
	}
}