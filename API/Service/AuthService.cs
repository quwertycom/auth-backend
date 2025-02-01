using System.Text.RegularExpressions;
using API.Common.Enums;
using API.Common.Helpers;
using API.Contracts.Requests.Auth;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service;

public interface IAuthService
{
	Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(RegisterRequest request);
	Task<(bool isSuccess, string status, string message, long? verificationSessionID)> VerifyEmailAsync(VerifyEmailRequest request);
	Task<(bool isSuccess, string status, string message, string? accessToken, string? refreshToken)> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
	private readonly AuthDbContext _dbContext;
	private readonly IUserInfoRepository _userInfoRepository;
	private readonly ITokenRepository _tokenRepository;
	private readonly ISessionRepository _sessionRepository;
	public AuthService(IUserInfoRepository userInfoRepository, ITokenRepository tokenRepository, ISessionRepository sessionRepository)
	{
		_dbContext = dbContext;
		_userInfoRepository = userInfoRepository;
		_tokenRepository = tokenRepository;
		_sessionRepository = sessionRepository;
	}

	public async Task<(bool isSuccess, string status, string message, long? verificationSessionID)> RegisterUserAsync(RegisterRequest request)
	{
		try
		{
			var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
			var usernameRegex = new Regex(@"^[a-zA-Z0-9_-]{3,50}$");
			var phoneRegex = new Regex(@"^\+?\d{1,4}[-.\s]?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,4}$");

			if (!emailRegex.IsMatch(request.Email))
			{
				return (false, "INVALID_EMAIL", "Invalid email format.", null);
			}

			if (!usernameRegex.IsMatch(request.Username))
			{
				return (false, "INVALID_USERNAME", "Invalid username format.", null);
			}

			if (request.PhoneNumber != null && !phoneRegex.IsMatch(request.PhoneNumber))
			{
				return (false, "INVALID_PHONE_NUMBER", "Invalid phone number format.", null);
			}
			var user = _userInfoRepository.GetUserByUserName(request.Username);
			var usernameExists = (user != null) ? true : false;
			if (usernameExists)
			{
				return (false, "USERNAME_TAKEN", "Username already exists, please try a different username.", null);
			}
			var email = await _userInfoRepository.GetEmailModelByEmail(request.Email);
			var emailExists = (email != null && email.State != EmailState.Created && email.State != EmailState.Deleted) ? true : false;

			if (emailExists)
			{
				return (false, "EMAIL_TAKEN", "Email already exists, please try a different email.", null);
			}
			var phoneNumber = await _userInfoRepository.GetPhoneNumberModelByPhoneNumber(request.PhoneNumber);
			var phoneNumberExists = (phoneNumber != null && phoneNumber.State != PhoneState.Created && phoneNumber.State != PhoneState.Deleted && phoneNumber.Type != PhoneType.Recovery) ? true : false;

			if (phoneNumberExists)
			{
				return (false, "PHONE_NUMBER_TAKEN", "Phone number already exists, please try a different phone number.", null);
			}

			if (request.Password.Length < 8)
			{
				return (false, "PASSWORD_TOO_SHORT", "Password must be at least 8 characters long.", null);
			}

			var hashedPassword = PasswordHasher.Hash(request.Password);

			var newUser = new User
			{
				Username = request.Username,
				FirstName = request.FirstName,
				LastName = request.LastName,
				BirthDate = request.BirthDate,
				PasswordHash = hashedPassword.hash,
				PasswordSalt = hashedPassword.salt,
			};

			var newEmail = new EmailAddress
			{
				Email = request.Email,
				Type = EmailType.Primary,
				State = EmailState.Created,
				User = newUser,
			};

			var otp = OTPGenerator.GenerateOTP();

			var otpSession = new VerificationSession
			{
				Email = newEmail,
				User = newUser,
				Code = otp,
				IsUsed = false,
			};

			_dbContext.Users.Add(newUser);
			_dbContext.UserEmails.Add(newEmail);
			_dbContext.VerificationSessions.Add(otpSession);
			await _dbContext.SaveChangesAsync();

			await EmailSender.SendOtpEmailAsync(newEmail.Email, otp);

			return (true, "OTP_SENT", "8 Digits code has been sent to your email. Please verify your email and login.", otpSession.Id);
		}
		catch
		{
			return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null);
		}
	}

	public async Task<(bool isSuccess, string status, string message, long? verificationSessionID)> VerifyEmailAsync(VerifyEmailRequest request)
	{
		try
		{
			var verificationSession = await _sessionRepository.GetSeession(request.VerificationSessionID);

			Console.WriteLine(verificationSession?.EmailId.ToString() ?? "No sessions found");
			switch (verificationSession)
			{
				case null:
					return (false, "NOT_FOUND", "Verification session not found.", null);
				case var session when session.Code != request.OTP:
					return (false, "INVALID_OTP", "Invalid OTP.", null);
				case var session when session.IsUsed:
					return (false, "ALREADY_USED", "OTP already used.", null);
				case var session when session.CreatedAt.AddMinutes(session.ExpiryMinutes) < DateTime.UtcNow:
					return (false, "EXPIRED", "OTP expired.", null);
			}

			var email = await _userInfoRepository.GetEmailModelByEmail(request.Email);

			if (email == null)
			{
				return (false, "NOT_FOUND", "Email not found.", null);
			}

			System.Console.WriteLine("verificationSession.UserId: " + verificationSession.UserId);
			System.Console.WriteLine("email.UserId: " + email.UserId);

			if (verificationSession.UserId != email.UserId)
			{
				return (false, "INVALID_SESSION", "Invalid verification session for this email.", null);
			}

			verificationSession.IsUsed = true;
			await _userInfoRepository.ChangeEmailState(email.Id, EmailState.Verified);

			return (true, "SUCCESS", "Email verified successfully.", verificationSession.Id);
		}
		catch
		{
			return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null);
		}
	}

	public async Task<(bool isSuccess, string status, string message, string? accessToken, string? refreshToken)> LoginAsync(LoginRequest request)
	{
		try
		{
			var user = await _userInfoRepository.GetUserByUsername(request.Username);

			if (user == null)
			{
				return (false, "NOT_FOUND", "User not found.", null, null);
			}

			if (!PasswordHasher.Compare(request.Password, user.PasswordHash, user.PasswordSalt))
			{
				return (false, "INVALID_PASSWORD", "Invalid password.", null, null);
			}

			string accessTokenString;
			string refreshTokenString;

			var refreshTokenResponse = JWT.GenerateRefreshToken(TokenTarget.User, (user.Id, null, null));

			if (refreshTokenResponse.isSuccess && refreshTokenResponse.token != null)
			{
				refreshTokenString = refreshTokenResponse.token;

				var accessTokenResponse = JWT.GenerateAccessToken(refreshTokenString);

				if (accessTokenResponse.isSuccess && accessTokenResponse.token != null)
				{
					accessTokenString = accessTokenResponse.token;
				}
				else
				{
					return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null, null);
				}
			}
			else
			{
				return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null, null);
			}

			var session = new Session
			{
				Target = SessionTarget.User,
				User = user
			};


			Token refreshToken = new Token
			{
				TokenString = refreshTokenString,
				Type = TokenType.Refresh,
				Target = TokenTarget.User,
				Session = session,
				User = user,
				CreatedAt = DateTime.UtcNow,
			};

			Token accessToken = new Token
			{
				TokenString = accessTokenString,
				Type = TokenType.Access,
				Target = TokenTarget.User,
				ParentToken = refreshToken,
				Session = session,
				User = user,
				CreatedAt = DateTime.UtcNow,
			};

			await _tokenRepository.AddToken(refreshToken);
			await _tokenRepository.AddToken(accessToken);
			await _sessionRepository.AddSession(session);

			return (true, "SUCCESS", "Login successful.", accessTokenString, refreshTokenString);
		}
		catch
		{
			return (false, "INTERNAL_SERVER_ERROR", "Internal server error, please try again later or contact support if the issue persists.", null, null);
		}
	}
}