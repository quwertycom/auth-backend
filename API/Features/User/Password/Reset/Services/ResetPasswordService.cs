using API.Features.User.Password.Reset.Interfaces;
using API.Features.User.Password.Reset.Models.Services;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Email;
using API.Shared.Interfaces.Security;
using API.Shared.Enums.Entities.User;

namespace API.Features.User.Password.Reset.Services;

public class ResetPasswordService : IResetPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IHasher _hasher;
    private readonly IEmailSender _emailSender;

    public ResetPasswordService(IUserRepository userRepository, IVerificationRepository verificationRepository, IRandomGenerator randomGenerator, IHasher hasher, IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _randomGenerator = randomGenerator;
        _hasher = hasher;
        _emailSender = emailSender;
    }

    public async Task<RequestPasswordResetResult> RequestPasswordResetViaEmailAsync(string email, CancellationToken cancellationToken)
    {
        try {
            var emailAdress = await _userRepository.GetEmailAdressByEmailStringAsync(email, includeUser: true);

            if (emailAdress == null) {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Email not found",
                    HttpStatusCode = 404
                };
            } else if (emailAdress.User == null)
            {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "User not found",
                    HttpStatusCode = 404
                };
            } else if (emailAdress.State != EmailState.Active)
            {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "This email is not active and cannot be used to reset your password",
                    HttpStatusCode = 400
                };
            }

            var user = emailAdress.User;

            var code = _randomGenerator.GenerateAlphanumericCode(64);
            var codeHash = _hasher.Hash(code, "");

            var newRequest = new PasswordResetRequest
            {
                CodeHash = codeHash.Hash,
                EmailAddress = emailAdress,
                User = user
            };

            var emailSent = await _emailSender.SendResetPasswordEmailAsync(emailAdress.Value, code, "en");

            if (!emailSent) {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Email cannot be sent",
                    HttpStatusCode = 500
                };
            }

            await _verificationRepository.AddPasswordResetRequestAsync(newRequest);

            return new RequestPasswordResetResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Password reset request sent",
                HttpStatusCode = 200
            };
            
        } catch (Exception ex) {
            return new RequestPasswordResetResult
            {
                IsSuccess = false,
                Message = ex.Message,
                Status = "ERROR",
                HttpStatusCode = 500
            };
        }
    }

    public async Task<RequestPasswordResetResult> RequestPasswordResetViaUsernameAsync(string username, CancellationToken cancellationToken)
    {
        try {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null) {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "User not found",
                    HttpStatusCode = 404
                };
            }

            var emailAdress = await _userRepository.GetUserPrimaryEmailAddressAsync(user.Id);

            if (emailAdress == null) {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Email not found",
                    HttpStatusCode = 404
                };
            } else if (emailAdress.User == null)
            {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "User not found",
                    HttpStatusCode = 404
                };
            } else if (emailAdress.State != EmailState.Active)
            {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "This email is not active and cannot be used to reset your password",
                    HttpStatusCode = 400
                };
            }

            var code = _randomGenerator.GenerateAlphanumericCode(64);
            var codeHash = _hasher.Hash(code, "");

            var newRequest = new PasswordResetRequest
            {
                CodeHash = codeHash.Hash,
                EmailAddress = emailAdress,
                User = user
            };

            var emailSent = await _emailSender.SendResetPasswordEmailAsync(emailAdress.Value, code, "en");

            if (!emailSent) {
                return new RequestPasswordResetResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Email cannot be sent",
                    HttpStatusCode = 500
                };
            }

            await _verificationRepository.AddPasswordResetRequestAsync(newRequest);

            return new RequestPasswordResetResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Password reset request sent",
                HttpStatusCode = 200
            };
            
        } catch (Exception ex) {
            return new RequestPasswordResetResult
            {
                IsSuccess = false,
                Message = ex.Message,
                Status = "ERROR",
                HttpStatusCode = 500
            };
        }
    }

    public async Task<CheckRequestStatusResult> CheckRequestStatusAsync(string code, CancellationToken cancellationToken)
    {
        try {
            var codeHash = _hasher.Hash(code, "");
            var request = await _verificationRepository.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash);

            if (request == null) {
                return new CheckRequestStatusResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Request not found",
                    HttpStatusCode = 404
                };
            }

            return new CheckRequestStatusResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Request found",
                HttpStatusCode = 200,
                IsExpired = request.ExpiresAt < DateTime.UtcNow,
                IsUsed = request.IsUsed,
            };
        } catch (Exception ex) {
            return new CheckRequestStatusResult
            {
                IsSuccess = false,
                Message = ex.Message,
                Status = "ERROR",
                HttpStatusCode = 500
            };
            
        }
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(string code, string newPassword, CancellationToken cancellationToken)
    {
        try {
            // Generate code hash using empty salt
            var codeHash = _hasher.Hash(code, "");
            var request = await _verificationRepository.GetPasswordResetRequestByCodeHashAsync(codeHash.Hash, includeEmailAddress: true);
            
            if (request == null) {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Request not found",
                    HttpStatusCode = 404
                };
            } 
            
            if (request.ExpiresAt < DateTime.UtcNow) {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Request expired",
                    HttpStatusCode = 400
                };
            } 
            
            if (request.IsUsed) {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Request already used",
                    HttpStatusCode = 400
                };
            }

            // Get user information
            var email = await _userRepository.GetEmailAdressByIdAsync(request.EmailAddress.Id, includeUser: true);
            
            if (email == null) {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "Email not found",
                    HttpStatusCode = 404
                };
            } 
            
            if (email.User == null) {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "User not found",
                    HttpStatusCode = 404
                };
            }
            
            if (email.User.Id != request.User.Id) {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Status = "ERROR",
                    Message = "User mismatch",
                    HttpStatusCode = 400
                };
            }
            
            // Generate a new hash and salt for the password
            // Don't pass a custom salt - let the hasher generate a fresh one
            var hashedPassword = _hasher.Hash(newPassword);
            
            // Update the user's password with both the new hash and salt
            await _userRepository.UpdateUserPasswordAsync(email.User.Id, hashedPassword.Hash, hashedPassword.Salt);
            
            // Mark the request as used
            await _verificationRepository.MarkPasswordResetRequestAsUsedAsync(request.Id);

            return new ResetPasswordResult
            {
                IsSuccess = true,
                Status = "SUCCESS",
                Message = "Password reset successfully",
                HttpStatusCode = 200
            };
        } catch (Exception ex) {
            return new ResetPasswordResult
            {
                IsSuccess = false,
                Message = ex.Message,
                Status = "ERROR",
                HttpStatusCode = 500
            };
        }
    }
}