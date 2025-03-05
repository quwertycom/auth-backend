using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Database.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user) {
      try {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: {ex.Message}", ex);
      }
    }

    public async Task AddEmailAsync(EmailAddress email) {
      try {
        await _context.EmailAddresses.AddAsync(email);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to add email: {ex.Message}", ex);
      }
   }

   public async Task AddPhoneNumberAsync(PhoneNumber phoneNumber) {
      try {
        await _context.PhoneNumbers.AddAsync(phoneNumber);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to add phone number: {ex.Message}", ex);
      }
   }

   public async Task<User?> GetUserByUsernameAsync(string username, bool includeEmails = false, bool includePhoneNumbers = false, bool includeSessions = false, bool includeAccounts = false) {
      try {
        IQueryable<User> query = _context.Users;

        if (includeEmails) {
            query = query.Include(user => user.EmailAddresses);
        }

        if (includePhoneNumbers) {
            query = query.Include(user => user.PhoneNumbers);
        }

        if (includeSessions) {
            query = query.Include(user => user.Sessions);
        }

        if (includeAccounts) {
            query = query.Include(user => user.Accounts);
        }
        
        return await query.FirstOrDefaultAsync(u => u.Username == username);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to get user by username: {ex.Message}", ex);
      }
   }

   public async Task<User?> GetUserByEmailAsync(string email, bool includeEmails = false, bool includePhoneNumbers = false, bool includeSessions = false, bool includeAccounts = false) {
      try {
        IQueryable<User> query = _context.Users;

        if (includeEmails) {
            query = query.Include(user => user.EmailAddresses);
        }

        if (includePhoneNumbers) {
            query = query.Include(user => user.PhoneNumbers);
        }

        if (includeSessions) {
            query = query.Include(user => user.Sessions);
        }

        if (includeAccounts) {
            query = query.Include(user => user.Accounts);
        }
        
        return await query.FirstOrDefaultAsync(u => u.EmailAddresses.Any(e => e.Value == email));
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to get user by email: {ex.Message}", ex);
      }
   }

   public async Task<User?> GetUserByIdAsync(long id, bool includeEmails = false, bool includePhoneNumbers = false, bool includeSessions = false, bool includeAccounts = false) {
      try {
        IQueryable<User> query = _context.Users;

        if (includeEmails) {
            query = query.Include(user => user.EmailAddresses);
        }

        if (includePhoneNumbers) {
            query = query.Include(user => user.PhoneNumbers);
        }
        
        if (includeSessions) {
            query = query.Include(user => user.Sessions);
        }

        if (includeAccounts) {
            query = query.Include(user => user.Accounts);
        }
        
        return await query.FirstOrDefaultAsync(u => u.Id == id);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to get user by id: {ex.Message}", ex);
      }
   }

   public async Task<EmailAddress?> GetUserPrimaryEmailAddressAsync(long userId, bool includeUser = false) {
      try {
        IQueryable<EmailAddress> query = _context.EmailAddresses;

        if (includeUser) {
            query = query.Include(email => email.User);
        }
        
        return await query.FirstOrDefaultAsync(e => e.UserId == userId && e.Type == EmailType.Primary);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to get user primary email address: {ex.Message}", ex);
      }
   }

   public async Task<EmailAddress?> GetEmailAdressByIdAsync(long id, bool includeUser = false) {
      try {
        IQueryable<EmailAddress> query = _context.EmailAddresses;

        if (includeUser) {
            query = query.Include(email => email.User);
        }
        
        return await query.FirstOrDefaultAsync(e => e.Id == id);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to get email address by id: {ex.Message}", ex);
      }
   }

   public async Task<EmailAddress?> GetEmailAdressByEmailStringAsync(string email, bool includeUser = false) {
      try {
        IQueryable<EmailAddress> query = _context.EmailAddresses;

        if (includeUser) {
            query = query.Include(email => email.User);
        }
        
        return await query.FirstOrDefaultAsync(e => e.Value == email);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to get email address by email string: {ex.Message}", ex);
      }
   }
   
   public async Task<bool> EmailAdressExistsAsync(string email) {
      try {
        return await _context.EmailAddresses.AnyAsync(e => e.Value == email);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to check if email address exists: {ex.Message}", ex);
      }
   }
   
   public async Task<bool> UsernameExistsAsync(string username) {
      try {
        return await _context.Users.AnyAsync(u => u.Username == username);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to check if username exists: {ex.Message}", ex);
      }
   }

   public async Task<bool> PhoneNumberExistsAsync(string phoneNumber) {
      try {
        return await _context.PhoneNumbers.AnyAsync(p => p.Value == phoneNumber);
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to check if phone number exists: {ex.Message}", ex);
      }
   }
   public async Task UpdateUserStateAsync(long userId, UserState newState) {
      try {
        var user = await GetUserByIdAsync(userId);
        if (user == null) {
          throw new Exception("NOT_FOUND: User not found");
        }
        user.State = newState;
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to update user state: {ex.Message}", ex);
      }
   }

   public async Task UpdateEmailStateAsync(long emailAdressId, EmailState newState) {
      try {
        var email = await GetEmailAdressByIdAsync(emailAdressId);
        if (email == null) {
          throw new Exception("NOT_FOUND: Email address not found");
        }
        email.State = newState;
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to update email state: {ex.Message}", ex);
      }
   }

   public async Task ChangeUserPrimaryEmailAddressAsync(long userId, long newEmailAdressId) {
      try {
        var user = await GetUserByIdAsync(userId);
        if (user == null) {
          throw new Exception("NOT_FOUND: User not found");
        }

        var newPrimaryEmail = await GetEmailAdressByIdAsync(newEmailAdressId);
        if (newPrimaryEmail == null) {
            throw new Exception("NOT_FOUND: New email address not found");
        }

        var currentPrimaryEmail = await GetUserPrimaryEmailAddressAsync(userId);
        if (currentPrimaryEmail != null) {
            currentPrimaryEmail.Type = EmailType.Other;
        }

        newPrimaryEmail.Type = EmailType.Primary;
        await _context.SaveChangesAsync();

      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to change user primary email address: {ex.Message}", ex);
      }
   }

   public async Task UpdateUserPasswordAsync(long userId, string newHash, string newSalt) {
      try {
        var user = await GetUserByIdAsync(userId);
        if (user == null) {
          throw new Exception("NOT_FOUND: User not found");
        }
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to update user password: {ex.Message}", ex);
      }
   }

   public async Task UpdateUserLastLoginAsync(long userId) {
      try {
        var user = await GetUserByIdAsync(userId);
        if (user == null) {
          throw new Exception("NOT_FOUND: User not found");
        }
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to update user last login: {ex.Message}", ex);
      }
   }

   public async Task RemoveEmailAddressAsync(long emailAdressId) {
      try {
        var email = await GetEmailAdressByIdAsync(emailAdressId);
        if (email == null) {
          throw new Exception("NOT_FOUND: Email address not found");
        }
        _context.EmailAddresses.Remove(email);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to remove email address: {ex.Message}", ex);
      }
   }

   public async Task RemoveUserAsync(long userId) {
      try {
        var user = await GetUserByIdAsync(userId);
        if (user == null) {
          throw new Exception("NOT_FOUND: User not found");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) {
        throw new Exception($"ERROR: Failed to remove user: {ex.Message}", ex);
      }
   }
}