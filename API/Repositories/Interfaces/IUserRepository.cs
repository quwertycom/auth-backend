using API.Models;
using API.Common.Enums;

namespace API.Repositories.Interfaces;

public interface IUserRepository
{
    public Task AddUser(User user);
    public Task AddEmail(EmailAddress emailAddress);
    public Task<User?> GetUserByUsername(string Username);
    public Task<EmailAddress?> GetEmailModelByEmail(string Email);
    public Task<PhoneNumber?> GetPhoneNumberModelByPhoneNumber(string PhoneNumber);
    public Task ChangeEmailState(long EmailId, EmailState newState);
    public Task RemoveEmailById(long Id);
    public Task RemovePhoneNumberById(long Id);
    public Task<(ResetPasswordRequest? request, string code)> SendResetPasswordRequest(long UserId);
}