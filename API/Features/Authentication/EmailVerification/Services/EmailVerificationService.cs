using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.EmailVerification.Models.Services;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Email;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;

namespace API.Features.Authentication.EmailVerification.Services;

public class EmailVerificationService : IEmailVerificationService {

  private readonly IUserRepository _userRepository;
  private readonly IVerificationRepository _verificationRepository;
  private readonly IRandomGenerator _randomGenerator;
  private readonly IEmailSender _emailSender;

  public EmailVerificationService(
    IUserRepository userRepository, 
    IVerificationRepository verificationRepository, 
    IRandomGenerator randomGenerator,
    IEmailSender emailSender) 
  {
    _userRepository = userRepository;
    _verificationRepository = verificationRepository;
    _randomGenerator = randomGenerator;
    _emailSender = emailSender;
  }

  public async Task<RequestEmailVerificationResult> RequestEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken) {
    try {
      var email = await _userRepository.GetEmailAdressByEmailStringAsync(emailAddress);
      if (email == null) {
        return new RequestEmailVerificationResult { IsSuccess = false, Status = "EMAIL_NOT_FOUND", Message = "Email address does not exist" };
      }

      var user = await _userRepository.GetUserByIdAsync(email.UserId);
      if (user == null) {
        return new RequestEmailVerificationResult { IsSuccess = false, Status = "USER_NOT_FOUND", Message = "User does not exist" };
      }

      if (email.State != EmailState.PendingVerification) {
        return new RequestEmailVerificationResult { IsSuccess = false, Status = "EMAIL_NOT_VERIFIED", Message = "Email address is not verified" };
      }

      var verificationCode = _randomGenerator.GenerateNumberCode(8);

      var newSession = new EmailVerificationRequest {
        Code = verificationCode,
        User = user,
        EmailAddress = email,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        CreatedAt = DateTime.UtcNow
      };

      await _verificationRepository.AddEmailVerificationRequestAsync(newSession);

      var emailVerificationSessionId = newSession.Id.ToString();

      bool emailSent = await _emailSender.SendOtpEmailAsync(
        emailAddress, 
        verificationCode,
        user.FirstName,
        "en"
      );

      if (!emailSent) {
        return new RequestEmailVerificationResult { 
          IsSuccess = false, 
          Status = "EMAIL_SENDING_FAILED", 
          Message = "Failed to send verification email" 
        };
      }

      return new RequestEmailVerificationResult { IsSuccess = true, Status = "SUCCESS", Message = "Email verification request created", EmailVerificationSessionId = newSession.Id.ToString() };
      
    }
    catch (Exception ex) {
      return new RequestEmailVerificationResult { IsSuccess = false, Status = "ERROR", Message = ex.Message };
    }
  }

  public async Task<GetSessionStatusResult> GetSessionStatusAsync(long sessionId, string email, CancellationToken cancellationToken)
    {
        try {
          var session = await _verificationRepository.GetEmailVerificationRequestByIdAsync(sessionId);
          if (session == null) {
            return new GetSessionStatusResult {
              IsSuccess = false, 
              Status = "SESSION_NOT_FOUND", 
              Message = "Session not found",
              HttpStatusCode = 404
            };
          }

          var sessionEmail = await _userRepository.GetEmailAdressByIdAsync(session.EmailId);

          if (sessionEmail == null || sessionEmail.Value != email) {
            return new GetSessionStatusResult {
              IsSuccess = false,
              Status = "EMAIL_MISMATCH",
              Message = "Email address does not match"
            };
          } else if (session.IsUsed) {
            return new GetSessionStatusResult {
              IsSuccess = false,
              Status = "SESSION_USED",
              Message = "Session has already been used",
            };
          } if (session.ExpiresAt < DateTime.UtcNow) {
            return new GetSessionStatusResult {
              IsSuccess = false,
              Status = "SESSION_EXPIRED",
              Message = "Session has expired",
              HttpStatusCode = 410
            };
          } 

          return new GetSessionStatusResult {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "Session valid",
            IsValid = true
          };
        } catch (Exception ex) {
            return new GetSessionStatusResult {
              IsSuccess = false, 
              Status = "ERROR", 
              Message = ex.Message,
              HttpStatusCode = 500
            };
        }
    }

    public Task<VerifyEmailResult> VerifyEmailAsync(long sessionId, string code, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}