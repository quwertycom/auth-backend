using API.Infrastructure.Database.Entities.User;
using API.Shared.Enums.Entities.User;

namespace API.Shared.Interfaces.Database.Repositories;

/// <summary>
/// Repository for managing user and email address entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user to add.</param>
    public Task AddUserAsync(User user);

    /// <summary>
    /// Adds a new email address to the database.
    /// </summary>
    /// <param name="email">The email address to add.</param>
    public Task AddEmailAsync(EmailAddress email);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    public Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to get.</param>
    public Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to get.</param>
    public Task<User?> GetUserByIdAsync(long id);

    /// <summary>
    /// Gets the primary email address of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the primary email address of.</param>
    public Task<EmailAddress?> GetUserPrimaryEmailAddressAsync(long userId);

    /// <summary>
    /// Gets an email address by its ID.
    /// </summary>
    /// <param name="id">The ID of the email address to get.</param>
    public Task<EmailAddress?> GetEmailAdressByIdAsync(long id);

    /// <summary>
    /// Checks if an email address exists in the database.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    public Task<bool> EmailAdressExistsAsync(string email);

    /// <summary>
    /// Checks if a username exists in the database.
    /// </summary>
    /// <param name="username">The username to check.</param>
    public Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Updates the state of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="newState">The new state of the user.</param>
    public Task UpdateUserStateAsync(long userId, UserState newState);

    /// <summary>
    /// Updates the state of an email address.
    /// </summary>
    /// <param name="emailAdressId">The ID of the email address to update.</param>
    /// <param name="newState">The new state of the email address.</param>
    public Task UpdateEmailStateAsync(long emailAdressId, EmailState newState);

    /// <summary>
    /// Changes the primary email address of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to change the primary email address of.</param>
    /// <param name="newEmailAdressId">The ID of the new primary email address.</param>
    public Task ChangeUserPrimaryEmailAddressAsync(long userId, long newEmailAdressId);

    /// <summary>
    /// Updates the password of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update the password of.</param>
    /// <param name="newHash">The new hash of the password.</param>
    /// <param name="newSalt">The new salt of the password.</param>
    public Task UpdateUserPasswordAsync(long userId, string newHash, string newSalt);

    /// <summary>
    /// Updates the last login date of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update the last login date of.</param>
    public Task UpdateUserLastLoginAsync(long userId);

    /// <summary>
    /// Removes an email address from the database.
    /// </summary>
    /// <param name="emailAdressId">The ID of the email address to remove.</param>
    public Task RemoveEmailAddressAsync(long emailAdressId);

    /// <summary>
    /// Removes a user from the database.
    /// </summary>
    /// <param name="userId">The ID of the user to remove.</param>
    public Task RemoveUserAsync(long userId);
}
