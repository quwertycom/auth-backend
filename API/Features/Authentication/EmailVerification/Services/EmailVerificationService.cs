using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.EmailVerification.Models.Services;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Email;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;
using System.Runtime.InteropServices.Marshalling;

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

      var newRequest = new EmailVerificationRequest {
        Code = verificationCode,
        User = user,
        EmailAddress = email,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        CreatedAt = DateTime.UtcNow
      };

      await _verificationRepository.AddEmailVerificationRequestAsync(newRequest);

      var emailVerificationRequestId = newRequest.Id.ToString();

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

      return new RequestEmailVerificationResult { IsSuccess = true, Status = "SUCCESS", Message = "Email verification request created", RequestId = newRequest.Id.ToString() };
      
    }
    catch (Exception ex) {
      return new RequestEmailVerificationResult { IsSuccess = false, Status = "ERROR", Message = ex.Message };
    }
  }

  public async Task<GetRequestStatusResult> GetRequestStatusAsync(string requestId, string email, CancellationToken cancellationToken)
    {
        try {
          var request = await _verificationRepository.GetEmailVerificationRequestByIdAsync(long.Parse(requestId));
          if (request == null) {
            return new GetRequestStatusResult {
              IsSuccess = false, 
              Status = "REQUEST_NOT_FOUND", 
              Message = "Request not found",
              HttpStatusCode = 404
            };
          }

          var requestEmail = await _userRepository.GetEmailAdressByIdAsync(request.EmailId);

          if (requestEmail == null || requestEmail.Value != email) {
            return new GetRequestStatusResult {
              IsSuccess = false,
              Status = "EMAIL_MISMATCH",
              Message = "Email address does not match"
            };
          } else if (request.IsUsed) {
            return new GetRequestStatusResult {
              IsSuccess = false,
              Status = "REQUEST_USED",
              Message = "Request has already been used",
            };
          } else if (request.ExpiresAt < DateTime.UtcNow) {
            return new GetRequestStatusResult {
              IsSuccess = false,
              Status = "REQUEST_EXPIRED",
              Message = "Request has expired",
              HttpStatusCode = 410
            };
          } 

          return new GetRequestStatusResult {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "Request valid",
            IsValid = true
          };
        } catch (Exception ex) {
            return new GetRequestStatusResult {
              IsSuccess = false, 
              Status = "ERROR", 
              Message = ex.Message,
              HttpStatusCode = 500
            };
        }
    }

    public async Task<VerifyEmailResult> VerifyEmailAsync(string requestId, string code, CancellationToken cancellationToken)
    {
        try {
          var request = await _verificationRepository.GetEmailVerificationRequestByIdAsync(long.Parse(requestId));

          if (request == null) {
            return new VerifyEmailResult {
              IsSuccess = false,
              Status = "REQUEST_NOT_FOUND",
              Message = "Request not found",
              HttpStatusCode = 404
            };
          }

          if (request.IsUsed) {
            return new VerifyEmailResult {
              IsSuccess = false,
              Status = "REQUEST_USED",
              Message = "Request has already been used",
              HttpStatusCode = 400
            };
          }

          if (request.ExpiresAt < DateTime.UtcNow) {
            return new VerifyEmailResult {
              IsSuccess = false,
              Status = "REQUEST_EXPIRED",
              Message = "Request has expired",
              HttpStatusCode = 410
            };
          }

          if (request.Code != code) {
            return new VerifyEmailResult {
              IsSuccess = false,
              Status = "CODE_MISMATCH",
              Message = "Invalid verification code",
              HttpStatusCode = 400
            };
          }

          await _verificationRepository.MarkEmailVerificationRequestAsUsedAsync(long.Parse(requestId));
          await _userRepository.UpdateUserStateAsync(request.User.Id, UserState.Active);

          return new VerifyEmailResult {
            IsSuccess = true,
            Status = "SUCCESS",
            Message = "Email verified successfully",
            RequestId = requestId
          };
        } catch (Exception ex) {
          return new VerifyEmailResult {
            IsSuccess = false,
            Status = "ERROR",
            Message = ex.Message,
            HttpStatusCode = 500
          };
        }
    }
}