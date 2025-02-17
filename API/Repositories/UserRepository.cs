using API.Common.Enums;
using API.Common.Helpers;
using API.Data;
using API.Models;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _Context;
    public UserRepository(AuthDbContext context)
    {
        _Context = context;
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
    public async Task<EmailAddress?> GetEmailModelByEmail(string Email)
    {
        return await _Context.UserEmails
                .Include(ue => ue.User)
                .FirstOrDefaultAsync(ue => ue.Email == Email);
    }
    public async Task<PhoneNumber?> GetPhoneNumberModelByPhoneNumber(string PhoneNumber)
    {
        return await _Context.UserPhoneNumbers.FirstOrDefaultAsync(x => x.Phone == PhoneNumber);
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
    public async Task<ResetPasswordRequest?> SendResetPasswordRequest(long UserId)
    {
        try
        {
            var user = await _Context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user != null)
            {
                var email = await _Context.UserEmails.FirstOrDefaultAsync(e => e.UserId == UserId && e.State == EmailState.Verified && e.Type == EmailType.Primary);
                if (email != null)
                {
                    var otp = RandomGenerator.GenerateNumberCode(8);
                    var resetPasswordRequest = new ResetPasswordRequest
                    {
                        User = user,
                        EmailAddress = email,
                        OTP = otp,
                        IsUsed = false,
                    };
                    await _Context.ResetPasswordRequests.AddAsync(resetPasswordRequest);
                    await _Context.SaveChangesAsync();
                    return resetPasswordRequest;
                }
            }
            return null;
        }
        catch { return null; }
    }
}
