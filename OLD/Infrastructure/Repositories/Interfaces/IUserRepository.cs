using API.Core.Models;
using API.Core.Enums;

namespace API.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    public Task AddUser(User user);
    public Task AddEmail(EmailAddress emailAddress);
    public Task<User?> GetUserByUsername(string Username);
    public Task<User?> GetUserById(long Id);
    public Task<EmailAddress?> GetEmailModelByEmail(string Email);
    public Task<EmailAddress?> GetEmailModelByUserId(long UserId);
    public Task<PhoneNumber?> GetPhoneNumberModelByPhoneNumber(string PhoneNumber);
    public Task ChangeUserState(long UserId, UserState newState);
    public Task ChangeEmailState(long EmailId, EmailState newState);
    public Task RemoveEmailById(long Id);
    public Task RemovePhoneNumberById(long Id);
    public Task UpdateUserPassword(User user, string newHash, string newSalt);
}