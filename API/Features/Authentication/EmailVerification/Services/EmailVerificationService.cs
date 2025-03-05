using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.EmailVerification.Models.Services;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;
using API.Shared.Interfaces.Email;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.User;
using System.Runtime.InteropServices.Marshalling;

namespace API.Features.Authentication.EmailVerification.Services;

public class EmailVerificationService : IEmailVerificationService
{

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

    public async Task<RequestEmailVerificationResult> RequestEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken)
    {
        try
        {
            var email = await _userRepository.GetEmailAdressByEmailStringAsync(emailAddress);
            if (email == null)
            {
                return new RequestEmailVerificationResult { IsSuccess = false, Status = "EMAIL_NOT_FOUND", Message = "Email address does not exist" };
            }

            var user = await _userRepository.GetUserByIdAsync(email.UserId);
            if (user == null)
            {
                return new RequestEmailVerificationResult { IsSuccess = false, Status = "USER_NOT_FOUND", Message = "User does not exist" };
            }

            if (email.State != EmailState.PendingVerification)
            {
                return new RequestEmailVerificationResult { IsSuccess = false, Status = "EMAIL_NOT_VERIFIED", Message = "Email address is not verified" };
            }

            var verificationCode = _randomGenerator.GenerateNumberCode(8);

            var newRequest = new EmailVerificationRequest
            {
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

            if (!emailSent)
            {
                return new RequestEmailVerificationResult
                {
                    IsSuccess = false,
                    Status = "EMAIL_SENDING_FAILED",
                    Message = "Failed to send verification email"
                };
            }

            return new RequestEmailVerificationResult { IsSuccess = true, Status = "SUCCESS", Message = "Email verification request created", RequestId = newRequest.Id.ToString() };

        }
        catch (Exception ex)
        {
            return new RequestEmailVerificationResult { IsSuccess = false, Status = "ERROR", Message = ex.Message };
        }
    }

    public async Task<GetRequestStatusResult> GetRequestStatusAsync(string requestId, string email, CancellationToken cancellationToken)
    {
        try
        {
            var request = await _verificationRepository.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeEmailAddress: true);
            if (request == null)
            {
                return new GetRequestStatusResult
                {
                    IsSuccess = false,
                    Status = "REQUEST_NOT_FOUND",
                    Message = "Request not found",
                    HttpStatusCode = 404
                };
            }

            var requestEmail = await _userRepository.GetEmailAdressByIdAsync(request.EmailAddress.Id);

            if (requestEmail == null || requestEmail.Value != email)
            {
                return new GetRequestStatusResult
                {
                    IsSuccess = false,
                    Status = "EMAIL_MISMATCH",
                    Message = "Email address does not match"
                };
            }
            else if (request.IsUsed)
            {
                return new GetRequestStatusResult
                {
                    IsSuccess = false,
                    Status = "REQUEST_USED",
                    Message = "Request has already been used",
                };
            }
            else if (request.ExpiresAt < DateTime.UtcNow)
            {
                return new GetRequestStatusResult
                {
                    IsSuccess = false,
                    Status = "REQUEST_EXPIRED",
                    Message = "Request has expired",
                    HttpStatusCode = 410
                };
            }

            return new GetRequestStatusResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Request valid",
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new GetRequestStatusResult
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = ex.Message,
                HttpStatusCode = 500
            };
        }
    }

    public async Task<VerifyEmailResult> VerifyEmailAsync(string requestId, string code, CancellationToken cancellationToken)
    {
        try
        {
            var request = await _verificationRepository.GetEmailVerificationRequestByIdAsync(long.Parse(requestId), includeUser: true, includeEmailAddress: true);

            if (request == null)
            {
                return new VerifyEmailResult
                {
                    IsSuccess = false,
                    Status = "REQUEST_NOT_FOUND",
                    Message = "Request not found",
                    HttpStatusCode = 404
                };
            }

            if (request.IsUsed)
            {
                return new VerifyEmailResult
                {
                    IsSuccess = false,
                    Status = "REQUEST_USED",
                    Message = "Request has already been used",
                    HttpStatusCode = 400
                };
            }

            if (request.ExpiresAt < DateTime.UtcNow)
            {
                return new VerifyEmailResult
                {
                    IsSuccess = false,
                    Status = "REQUEST_EXPIRED",
                    Message = "Request has expired",
                    HttpStatusCode = 410
                };
            }

            if (request.Code != code)
            {
                return new VerifyEmailResult
                {
                    IsSuccess = false,
                    Status = "CODE_MISMATCH",
                    Message = "Invalid verification code",
                    HttpStatusCode = 400
                };
            }

            await _verificationRepository.MarkEmailVerificationRequestAsUsedAsync(long.Parse(requestId));
            await _userRepository.UpdateUserStateAsync(request.User.Id, UserState.Active);
            await _userRepository.UpdateEmailStateAsync(request.EmailAddress.Id, EmailState.Active);

            return new VerifyEmailResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Email verified successfully",
                RequestId = requestId
            };
        }
        catch (Exception ex)
        {
            return new VerifyEmailResult
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = ex.Message,
                HttpStatusCode = 500
            };
        }
    }

    public async Task<RequestNewCodeResult> RequestNewCodeAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            var userEmail = await _userRepository.GetEmailAdressByEmailStringAsync(email);

            if (userEmail == null)
            {
                return new RequestNewCodeResult
                {
                    IsSuccess = false,
                    Status = "EMAIL_NOT_FOUND",
                    Message = "Email address does not exist",
                    HttpStatusCode = 404
                };
            }
            else if (userEmail.State == EmailState.Active)
            {
                return new RequestNewCodeResult
                {
                    IsSuccess = false,
                    Status = "EMAIL_ALREADY_VERIFIED",
                    Message = "This email address is already verified, you can already use it to login.",
                    HttpStatusCode = 400
                };
            }
            else if (userEmail.State == EmailState.Blacklisted)
            {
                return new RequestNewCodeResult
                {
                    IsSuccess = false,
                    Status = "EMAIL_BLACKLISTED",
                    Message = "This email address is blacklisted, it cannot be used. Please contact support if you believe this is an error.",
                    HttpStatusCode = 400
                };
            }
            else if (userEmail.State == EmailState.Deleted || userEmail.State == EmailState.Disabled)
            {
                return new RequestNewCodeResult
                {
                    IsSuccess = false,
                    Status = "EMAIL_DISABLED",
                    Message = "This email address is disabled by the user, it cannot be used. Please contact support if you believe this is an error.",
                    HttpStatusCode = 400
                };
            }

            var createRequestResult = await RequestEmailVerificationAsync(email, cancellationToken);
            if (!createRequestResult.IsSuccess || createRequestResult.RequestId == null || createRequestResult.RequestId == string.Empty || createRequestResult.Status != "SUCCESS")
            {
                return new RequestNewCodeResult
                {
                    IsSuccess = false,
                    Status = createRequestResult.Status,
                    Message = createRequestResult.Message,
                    HttpStatusCode = 500
                };
            }
            else
            {
                return new RequestNewCodeResult
                {
                    IsSuccess = true,
                    Status = "SUCCESS",
                    Message = "New verification request created",
                    NewRequestId = createRequestResult.RequestId,
                    HttpStatusCode = 200
                };
            }
        }
        catch (Exception ex)
        {
            return new RequestNewCodeResult
            {
                IsSuccess = false,
                Status = "ERROR",
                Message = ex.Message,
                HttpStatusCode = 500
            };
        }
    }
}