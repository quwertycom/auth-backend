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
        try
        {
            await _Context.Users.AddAsync(user);
            await _Context.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }
    public async Task AddEmail(EmailAddress emailAddress)
    {
        try
        {
            await _Context.UserEmails.AddAsync(emailAddress);
            await _Context.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }
    public async Task<User?> GetUserByUsername(string Username)
    {
        try
        {
            return await _Context.Users.FirstOrDefaultAsync(u => u.Username == Username);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<User?> GetUserById(long Id)
    {
        try
        {
            return await _Context.Users.FirstOrDefaultAsync(u => u.Id == Id);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<EmailAddress?> GetEmailModelByEmail(string Email)
    {
        try
        {
            return await _Context.UserEmails
                .Include(ue => ue.User)
                .FirstOrDefaultAsync(ue => ue.Email == Email);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<EmailAddress?> GetEmailModelByUserId(long UserId)
    {
        try
        {
            return await _Context.UserEmails.FirstOrDefaultAsync(x => x.UserId == UserId);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<PhoneNumber?> GetPhoneNumberModelByPhoneNumber(string PhoneNumber)
    {
        try
        {
            return await _Context.UserPhoneNumbers.FirstOrDefaultAsync(x => x.Phone == PhoneNumber);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task ChangeUserState(long UserId, UserState newState)
    {
        try
        {
            var user = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
            {
                throw new Exception("NOT_FOUND");
            }
            else
            {
                user.State = newState;
                await _Context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task ChangeEmailState(long EmailId, EmailState newState)
    {
        try
        {
            var email = await _Context.UserEmails.FirstOrDefaultAsync(x => x.Id == EmailId);
            if (email != null)
            {
            email.State = newState;
                await _Context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task RemoveEmailById(long Id)
    {
        try
        {
            var Email = await _Context.UserEmails.FirstOrDefaultAsync(x => x.Id == Id);
            if (Email is null)
            {
                throw new Exception("NOT_FOUND");
            }
            _Context.UserEmails.Remove(Email);
            await _Context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task RemovePhoneNumberById(long Id)
    {
        try
        {
            var phoneNumber = await _Context.UserPhoneNumbers.FirstOrDefaultAsync(x => x.Id == Id);
            if (phoneNumber is null)
            {
                throw new Exception("Phone number not found");
            }
            _Context.UserPhoneNumbers.Remove(phoneNumber);
            await _Context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<(ResetPasswordRequest? request, string code)> SendResetPasswordRequest(long UserId)
    {
        try
        {
            var user = await _Context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user != null)
            {
                var email = await _Context.UserEmails.FirstOrDefaultAsync(e => e.UserId == UserId && e.State == EmailState.Verified && e.Type == EmailType.Primary);
                if (email != null)
                {
                    var code = RandomGenerator.GenerateAlphanumericCode(32);
                    var (codeHash, _) = Hasher.Hash(code, "");
                    var resetPasswordRequest = new ResetPasswordRequest
                    {
                        User = user,
                        EmailAddress = email,
                        CodeHash = codeHash,
                        IsUsed = false,
                    };
                    await _Context.ResetPasswordRequests.AddAsync(resetPasswordRequest);
                    await _Context.SaveChangesAsync();
                    return (resetPasswordRequest, code);
                }
                else
                {
                    throw new Exception("EMAIL_NOT_FOUND");
                }
            }
            else
            {
                throw new Exception("NOT_FOUND");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task UpdateUserPassword(User user, string newHash, string newSalt)
    {
        try
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            await _Context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log error: ex.Message
            throw new Exception("Concurrency error updating password");
        }
        catch (DbUpdateException ex)
        {
            // Log error: ex.InnerException?.Message
            throw new Exception("Database error updating password");
        }
    }
}
