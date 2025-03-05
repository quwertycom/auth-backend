using API.Infrastructure.Database.Entities.Verification;

namespace API.Shared.Interfaces.Database.Repositories;

/// <summary>
/// Repository for managing email verification and password reset requests.
/// </summary>
public interface IVerificationRepository
{
    /// <summary>
    /// Adds a new email verification request to the database.
    /// </summary>
    /// <param name="emailVerificationRequest">The email verification request to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddEmailVerificationRequestAsync(EmailVerificationRequest emailVerificationRequest);

    /// <summary>
    /// Adds a new password reset request to the database.
    /// </summary>
    /// <param name="passwordResetRequest">The password reset request to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddPasswordResetRequestAsync(PasswordResetRequest passwordResetRequest);

    /// <summary>
    /// Gets an email verification request by its ID.
    /// </summary>
    /// <param name="emailVerificationRequestId">The ID of the email verification request to get.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the email verification request if found, otherwise null.</returns>
    public Task<EmailVerificationRequest?> GetEmailVerificationRequestByIdAsync(long emailVerificationRequestId, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets an email verification request by its code.
    /// </summary>
    /// <param name="code">The code of the email verification request to get.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the email verification request if found, otherwise null.</returns>
    public Task<EmailVerificationRequest?> GetEmailVerificationRequestByCodeAsync(string code, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets an email verification request by its email ID.
    /// </summary>
    /// <param name="emailId">The email ID of the email verification request to get.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the email verification request if found, otherwise null.</returns>
    public Task<EmailVerificationRequest?> GetEmailVerificationRequestByEmailIdAsync(long emailId, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets an email verification request by the email string.
    /// </summary>
    /// <param name="email">The email string associated with the verification request.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the email verification request if found, otherwise null.</returns>
    public Task<EmailVerificationRequest?> GetEmailVerificationRequestByEmailStringAsync(string email, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets a password reset request by its code hash.
    /// </summary>
    /// <param name="codeHash">The code hash of the password reset request to get.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the password reset request if found, otherwise null.</returns>
    public Task<PasswordResetRequest?> GetPasswordResetRequestByCodeHashAsync(string codeHash, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets a password reset request by its email ID.
    /// </summary>
    /// <param name="emailId">The email ID of the password reset request to get.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the password reset request if found, otherwise null.</returns>
    public Task<PasswordResetRequest?> GetPasswordResetRequestByEmailIdAsync(long emailId, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets a password reset request by the email string.
    /// </summary>
    /// <param name="email">The email string associated with the password reset request.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the password reset request if found, otherwise null.</returns>
    public Task<PasswordResetRequest?> GetPasswordResetRequestByEmailStringAsync(string email, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets all password reset requests for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get password reset requests for.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of password reset requests.</returns>
    public Task<IEnumerable<PasswordResetRequest>> GetAllUserPasswordResetRequestsAsync(long userId, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Gets all active password reset requests for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get active password reset requests for.</param>
    /// <param name="includeUser">Whether to include the associated user in the query.</param>
    /// <param name="includeEmailAddress">Whether to include the associated email address in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of active password reset requests.</returns>
    public Task<IEnumerable<PasswordResetRequest>> GetUserActivePasswordResetRequestsAsync(long userId, bool includeUser = false, bool includeEmailAddress = false);

    /// <summary>
    /// Marks an email verification request as used.
    /// </summary>
    /// <param name="emailVerificationRequestId">The ID of the email verification request to mark as used.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task MarkEmailVerificationRequestAsUsedAsync(long emailVerificationRequestId);

    /// <summary>
    /// Marks a password reset request as used.
    /// </summary>
    /// <param name="passwordResetRequestId">The ID of the password reset request to mark as used.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task MarkPasswordResetRequestAsUsedAsync(long passwordResetRequestId);

    /// <summary>
    /// Removes an email verification request from the database.
    /// </summary>
    /// <param name="emailVerificationRequestId">The ID of the email verification request to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveEmailVerificationRequestAsync(long emailVerificationRequestId);

    /// <summary>
    /// Removes a password reset request from the database.
    /// </summary>
    /// <param name="passwordResetRequestId">The ID of the password reset request to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemovePasswordResetRequestAsync(long passwordResetRequestId);

    /// <summary>
    /// Removes all email verification requests for a user from the database.
    /// </summary>
    /// <param name="userId">The ID of the user to remove all email verification requests for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveAllUserEmailVerificationRequestsAsync(long userId);

    /// <summary>
    /// Removes all password reset requests for a user from the database.
    /// </summary>
    /// <param name="userId">The ID of the user to remove all password reset requests for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveAllUserPasswordResetRequestsAsync(long userId);
}
