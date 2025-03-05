using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;

namespace API.Shared.Interfaces.Database.Repositories;

/// <summary>
/// Repository for managing user entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddUserAsync(User user);

    /// <summary>
    /// Adds a new email address to the database.
    /// </summary>
    /// <param name="email">The email address to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddEmailAsync(EmailAddress email);

    /// <summary>
    /// Adds a new phone number to the database.
    /// </summary>
    /// <param name="phoneNumber">The phone number to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddPhoneNumberAsync(PhoneNumber phoneNumber);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <param name="includeEmails">Whether to include email addresses in the query.</param>
    /// <param name="includePhoneNumbers">Whether to include phone numbers in the query.</param>
    /// <param name="includeSessions">Whether to include sessions in the query.</param>
    /// <param name="includeAccounts">Whether to include accounts in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserByUsernameAsync(string username, bool includeEmails = false, bool includePhoneNumbers = false, bool includeSessions = false, bool includeAccounts = false);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to get.</param>
    /// <param name="includeEmails">Whether to include email addresses in the query.</param>
    /// <param name="includePhoneNumbers">Whether to include phone numbers in the query.</param>
    /// <param name="includeSessions">Whether to include sessions in the query.</param>
    /// <param name="includeAccounts">Whether to include accounts in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserByEmailAsync(string email, bool includeEmails = false, bool includePhoneNumbers = false, bool includeSessions = false, bool includeAccounts = false);

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to get.</param>
    /// <param name="includeEmails">Whether to include email addresses in the query.</param>
    /// <param name="includePhoneNumbers">Whether to include phone numbers in the query.</param>
    /// <param name="includeSessions">Whether to include sessions in the query.</param>
    /// <param name="includeAccounts">Whether to include accounts in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserByIdAsync(long id, bool includeEmails = false, bool includePhoneNumbers = false, bool includeSessions = false, bool includeAccounts = false);

    /// <summary>
    /// Gets the primary email address of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the primary email address of.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the primary email address if found, otherwise null.</returns>
    public Task<EmailAddress?> GetUserPrimaryEmailAddressAsync(long userId, bool includeUser = false);

    /// <summary>
    /// Gets an email address by its ID.
    /// </summary>
    /// <param name="id">The ID of the email address to get.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the email address if found, otherwise null.</returns>
    public Task<EmailAddress?> GetEmailAdressByIdAsync(long id, bool includeUser = false);

    /// <summary>
    /// Gets an email address by its email address.
    /// </summary>
    /// <param name="email">The email address of the email address to get.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the email address if found, otherwise null.</returns>
    public Task<EmailAddress?> GetEmailAdressByEmailStringAsync(string email, bool includeUser = false);

    /// <summary>
    /// Checks if an email address exists in the database.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>A task that represents the asynchronous operation. Returns true if the email address exists, otherwise false.</returns>
    public Task<bool> EmailAdressExistsAsync(string email);

    /// <summary>
    /// Checks if a username exists in the database.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>A task that represents the asynchronous operation. Returns true if the username exists, otherwise false.</returns>
    public Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Checks if a phone number exists in the database.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <returns>A task that represents the asynchronous operation. Returns true if the phone number exists, otherwise false.</returns>
    public Task<bool> PhoneNumberExistsAsync(string phoneNumber);

    /// <summary>
    /// Updates the state of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="newState">The new state of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateUserStateAsync(long userId, UserState newState);

    /// <summary>
    /// Updates the state of an email address.
    /// </summary>
    /// <param name="emailAdressId">The ID of the email address to update.</param>
    /// <param name="newState">The new state of the email address.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateEmailStateAsync(long emailAdressId, EmailState newState);

    /// <summary>
    /// Changes the primary email address of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to change the primary email address of.</param>
    /// <param name="newEmailAdressId">The ID of the new primary email address.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ChangeUserPrimaryEmailAddressAsync(long userId, long newEmailAdressId);

    /// <summary>
    /// Updates the password of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update the password of.</param>
    /// <param name="newHash">The new hash of the password.</param>
    /// <param name="newSalt">The new salt of the password.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateUserPasswordAsync(long userId, string newHash, string newSalt);

    /// <summary>
    /// Updates the last login date of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update the last login date of.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateUserLastLoginAsync(long userId);

    /// <summary>
    /// Removes an email address from the database.
    /// </summary>
    /// <param name="emailAdressId">The ID of the email address to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveEmailAddressAsync(long emailAdressId);

    /// <summary>
    /// Removes a user from the database.
    /// </summary>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveUserAsync(long userId);
}
