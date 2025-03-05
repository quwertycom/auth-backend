using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Interfaces.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Database.Repositories;

public class VerificationRepository : IVerificationRepository
{
    private readonly AuthDbContext _context;

    public VerificationRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddEmailVerificationRequestAsync(EmailVerificationRequest emailVerificationRequest)
    {
        try
        {
            await _context.EmailVerificationRequests.AddAsync(emailVerificationRequest);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to add email verification request: {ex.Message}", ex);
        }
    }

    public async Task AddPasswordResetRequestAsync(PasswordResetRequest passwordResetRequest)
    {
        try
        {
            await _context.PasswordResetRequests.AddAsync(passwordResetRequest);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to add password reset request: {ex.Message}", ex);
        }
    }

    public async Task<EmailVerificationRequest?> GetEmailVerificationRequestByIdAsync(long emailVerificationRequestId, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.EmailVerificationRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(e => e.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(e => e.EmailAddress);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == emailVerificationRequestId);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get email verification request by id: {ex.Message}", ex);
        }
    }

    public async Task<EmailVerificationRequest?> GetEmailVerificationRequestByCodeAsync(string code, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.EmailVerificationRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(e => e.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(e => e.EmailAddress);
            }

            return await query.FirstOrDefaultAsync(e => e.Code == code);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get email verification request by code: {ex.Message}", ex);
        }
    }

    public async Task<EmailVerificationRequest?> GetEmailVerificationRequestByEmailIdAsync(long emailId, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.EmailVerificationRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(e => e.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(e => e.EmailAddress);
            }

            return await query.FirstOrDefaultAsync(e => e.EmailId == emailId);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get email verification request by email id: {ex.Message}", ex);
        }
    }

    public async Task<EmailVerificationRequest?> GetEmailVerificationRequestByEmailStringAsync(string email, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.EmailVerificationRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(e => e.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(e => e.EmailAddress);
            }

            return await query.FirstOrDefaultAsync(e => e.EmailAddress.Value == email);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get email verification request by email string: {ex.Message}", ex);
        }
    }

    public async Task<PasswordResetRequest?> GetPasswordResetRequestByCodeHashAsync(string codeHash, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.PasswordResetRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(p => p.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(p => p.EmailAddress);
            }

            return await query.FirstOrDefaultAsync(p => p.CodeHash == codeHash);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get password reset request by code hash: {ex.Message}", ex);
        }
    }

    public async Task<PasswordResetRequest?> GetPasswordResetRequestByEmailIdAsync(long emailId, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.PasswordResetRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(p => p.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(p => p.EmailAddress);
            }

            return await query.FirstOrDefaultAsync(p => p.EmailId == emailId);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get password reset request by email id: {ex.Message}", ex);
        }
    }

    public async Task<PasswordResetRequest?> GetPasswordResetRequestByEmailStringAsync(string email, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.PasswordResetRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(p => p.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(p => p.EmailAddress);
            }
            return await query.FirstOrDefaultAsync(p => p.EmailAddress.Value == email);
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get password reset request by email string: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<PasswordResetRequest>> GetAllUserPasswordResetRequestsAsync(long userId, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.PasswordResetRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(p => p.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(p => p.EmailAddress);
            }

            return await query.Where(p => p.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get all user password reset requests: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<PasswordResetRequest>> GetUserActivePasswordResetRequestsAsync(long userId, bool includeUser = false, bool includeEmailAddress = false)
    {
        try
        {
            var query = _context.PasswordResetRequests.AsQueryable();
            if (includeUser)
            {
                query = query.Include(p => p.User);
            }
            if (includeEmailAddress)
            {
                query = query.Include(p => p.EmailAddress);
            }
            return await query.Where(p => p.UserId == userId && p.IsUsed == false && p.ExpiresAt > DateTime.UtcNow).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to get user active password reset requests: {ex.Message}", ex);
        }
    }

    public async Task MarkEmailVerificationRequestAsUsedAsync(long emailVerificationRequestId)
    {
        try
        {
            var request = await _context.EmailVerificationRequests.FindAsync(emailVerificationRequestId);
            if (request == null)
            {
                throw new Exception("NOT_FOUND: Email verification request not found");
            }
            request.IsUsed = true;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to mark email verification request as used: {ex.Message}", ex);
        }
    }

    public async Task MarkPasswordResetRequestAsUsedAsync(long passwordResetRequestId)
    {
        try
        {
            var request = await _context.PasswordResetRequests.FindAsync(passwordResetRequestId);
            if (request == null)
            {
                throw new Exception("NOT_FOUND: Password reset request not found");
            }
            request.IsUsed = true;
            request.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to mark password reset request as used: {ex.Message}", ex);
        }
    }

    public async Task RemoveEmailVerificationRequestAsync(long emailVerificationRequestId)
    {
        try
        {
            var request = await _context.EmailVerificationRequests.FindAsync(emailVerificationRequestId);
            if (request == null)
            {
                throw new Exception("NOT_FOUND: Email verification request not found");
            }
            _context.EmailVerificationRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to remove email verification request: {ex.Message}", ex);
        }
    }

    public async Task RemovePasswordResetRequestAsync(long passwordResetRequestId)
    {
        try
        {
            var request = await _context.PasswordResetRequests.FindAsync(passwordResetRequestId);
            if (request == null)
            {
                throw new Exception("NOT_FOUND: Password reset request not found");
            }
            _context.PasswordResetRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to remove password reset request: {ex.Message}", ex);
        }
    }

    public async Task RemoveAllUserEmailVerificationRequestsAsync(long userId)
    {
        try
        {
            var requests = await _context.EmailVerificationRequests.Where(e => e.UserId == userId).ToListAsync();
            _context.EmailVerificationRequests.RemoveRange(requests);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to remove all user email verification requests: {ex.Message}", ex);
        }
    }

    public async Task RemoveAllUserPasswordResetRequestsAsync(long userId)
    {
        try
        {
            var requests = await _context.PasswordResetRequests.Where(p => p.UserId == userId).ToListAsync();
            _context.PasswordResetRequests.RemoveRange(requests);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: Failed to remove all user password reset requests: {ex.Message}", ex);
        }
    }
}

