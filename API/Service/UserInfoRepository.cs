using API.Common.Enums;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public interface IUserInfoRepository
    {
        public Task<EmailAddress?> GetEmailModelByEmail(string Email);
        public Task ChangeEmailState(long EmailId, EmailState newState);
        public Task AddUser(User user);
        public Task AddEmail(EmailAddress emailAddress);
        public Task<User?> GetUserByUsername(string Username);
        public Task<PhoneNumber?> GetPhoneNumberModelByPhoneNumber(string PhoneNumber);
        public Task RemovePhoneNumberById(long Id);
        public Task RemoveEmailById(long Id);
        public Task<User?> GetUserByUserName(string Username);
    }
    public class UserInfoRepository : IUserInfoRepository
    {
        private readonly AuthDbContext _Context;
        public UserInfoRepository(AuthDbContext context)
        {
            _Context = context;
        }
        public async Task<EmailAddress?> GetEmailModelByEmail(string Email)
        {
            return await _Context.UserEmails
                .Include(ue => ue.User)
                .FirstOrDefaultAsync(ue => ue.Email == Email);
        }
        public async Task ChangeEmailState(long EmailId, EmailState newState)
        {
            var email = await _Context.UserEmails.FirstOrDefaultAsync(x => x.Id == EmailId);
            if (email != null)
            {
                email.State = newState;
                _Context.SaveChanges();
            }
        }
        public async Task<PhoneNumber?> GetPhoneNumberModelByPhoneNumber(string PhoneNumber)
        {
            return await _Context.UserPhoneNumbers.FirstOrDefaultAsync(x => x.Phone == PhoneNumber);
        }
        public async Task AddUser(User user)
        {
            await _Context.Users.AddAsync(user);
            await _Context.SaveChangesAsync();
        }
        public async Task AddEmail(EmailAddress emailAddress)
        {
            await _Context.UserEmails.AddAsync(emailAddress);
            await _Context.SaveChangesAsync();
        }
        public async Task<User?> GetUserByUsername(string Username)
        {
            return await _Context.Users.FirstOrDefaultAsync(u => u.Username == Username);
        }
        public async Task RemoveEmailById(long Id)
        {
            var Email = await _Context.UserEmails.FirstOrDefaultAsync(x => x.Id == Id);
            if (Email != null)
            {
                try
                {
                    _Context.UserEmails.Remove(Email);
                    await _Context.SaveChangesAsync();
                }
                catch { }
            }

        }
        public async Task RemovePhoneNumberById(long Id)
        {
            var phoneNumber = await _Context.UserPhoneNumbers.FirstOrDefaultAsync(x => x.Id == Id);
            if (phoneNumber != null)
            {
                try
                {
                    _Context.UserPhoneNumbers.Remove(phoneNumber);
                    await _Context.SaveChangesAsync();
                }
                catch { }
            }

        }
        public async Task<User?> GetUserByUserName(string Username)
        {
            return await _Context.Users.FirstOrDefaultAsync(u => u.Username == Username);
        }
    }
}
