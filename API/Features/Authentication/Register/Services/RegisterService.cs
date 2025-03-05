using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.Register.Interfaces;
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.Register.Models.Services;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Security;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;

namespace API.Features.Authentication.Register.Services;


public class RegisterService : IRegisterService
{
    private readonly IEmailVerificationService _emailVerificationService;

    private readonly IUserRepository _userRepository;

    private readonly IHasher _hasher;

    public RegisterService(IUserRepository userRepository, IHasher hasher, IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
        _userRepository = userRepository;
        _hasher = hasher;
    }

    public async Task<RegisterResult> RegisterUserAsync(RegisterRequest request, CancellationToken ct)
    {
        try
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                return new RegisterResult { IsSuccess = false, Status = "USERNAME_EXISTS" };
            }
            else if (await _userRepository.EmailAdressExistsAsync(request.Email))
            {
                return new RegisterResult { IsSuccess = false, Status = "EMAIL_EXISTS" };
            }
            else if (request.PhoneNumber != null && await _userRepository.PhoneNumberExistsAsync(request.PhoneNumber))
            {
                return new RegisterResult { IsSuccess = false, Status = "PHONE_NUMBER_EXISTS" };
            }

            var hashedPassword = _hasher.Hash(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = hashedPassword.hash,
                PasswordSalt = hashedPassword.salt,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                State = UserState.PendingVerification
            };

            await _userRepository.AddUserAsync(newUser);

            var newEmailAddress = new EmailAddress
            {
                UserId = newUser.Id,
                Value = request.Email,
                State = EmailState.PendingVerification,
                Type = EmailType.Primary,
                User = newUser
            };

            await _userRepository.AddEmailAsync(newEmailAddress);

            if (request.PhoneNumber != null)
            {
                var newPhoneNumber = new PhoneNumber
                {
                    UserId = newUser.Id,
                    Value = request.PhoneNumber,
                    State = PhoneState.PendingVerification,
                    Type = PhoneType.Primary,
                    User = newUser
                };

                await _userRepository.AddPhoneNumberAsync(newPhoneNumber);
            }

            var emailVerificationResult = await _emailVerificationService.RequestEmailVerificationAsync(request.Email, ct);

            if (!emailVerificationResult.IsSuccess || emailVerificationResult.RequestId == null)
            {
                return new RegisterResult { IsSuccess = false, Status = emailVerificationResult.Status, Message = emailVerificationResult.Message };
            }

            return new RegisterResult { IsSuccess = true, Status = "SUCCESS", Message = "User registered successfully", RequestId = emailVerificationResult.RequestId };
        }
        catch (Exception ex)
        {
            return new RegisterResult { IsSuccess = false, Status = "ERROR", Message = ex.Message };
        }
    }
}