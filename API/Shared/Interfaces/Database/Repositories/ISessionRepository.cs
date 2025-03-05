
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;

namespace API.Shared.Interfaces.Database.Repositories;

/// <summary>
/// Repository for managing session and token entities.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Adds a new session to the database.
    /// </summary>
    /// <param name="session">The session to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddSessionAsync(Session session);

    /// <summary>
    /// Adds a new token to the database.
    /// </summary>
    /// <param name="token">The token to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddTokenAsync(Token token);

    /// <summary>
    /// Gets a session by its ID.
    /// </summary>
    /// <param name="id">The ID of the session to get.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeTokens">Whether to include tokens in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the session if found, otherwise null.</returns>
    public Task<Session?> GetSessionByIdAsync(long id, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false);

    /// <summary>
    /// Gets a session by its token string.
    /// </summary>
    /// <param name="tokenString">The token string of the session to get.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeTokens">Whether to include tokens in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the session if found, otherwise null.</returns>
    public Task<Session?> GetSessionByTokenStringAsync(string tokenString, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false);

    /// <summary>
    /// Gets a session by its user ID.
    /// </summary>
    /// <param name="userId">The ID of the user to get sessions for.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeTokens">Whether to include tokens in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the session if found, otherwise null.</returns>
    public Task<Session?> GetSessionByUserIdAsync(long userId, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false);

    /// <summary>
    /// Gets all sessions for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get sessions for.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeTokens">Whether to include tokens in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of sessions.</returns>
    public Task<IEnumerable<Session>> GetAllUserSessionsAsync(long userId, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false);

    /// <summary>
    /// Gets all active sessions for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get active sessions for.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeTokens">Whether to include tokens in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of active sessions.</returns>
    public Task<IEnumerable<Session>> GetActiveUserSessionsAsync(long userId, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeTokens = false);

    /// <summary>
    /// Gets a token by its token string.
    /// </summary>
    /// <param name="tokenString">The token string of the token to get.</param>
    /// <param name="includeSession">Whether to include the session in the query.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeParentToken">Whether to include parent token in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the token if found, otherwise null.</returns>
    public Task<Token?> GetTokenByTokenStringAsync(string tokenString, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false);

    /// <summary>
    /// Gets all tokens for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get tokens for.</param>
    /// <param name="includeSession">Whether to include the session in the query.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeParentToken">Whether to include parent token in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of tokens.</returns>
    public Task<IEnumerable<Token>> GetAllUserTokensAsync(long userId, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false);

    /// <summary>
    /// Gets all active tokens for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get active tokens for.</param>
    /// <param name="includeSession">Whether to include the session in the query.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeParentToken">Whether to include parent token in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of active tokens.</returns>
    public Task<IEnumerable<Token>> GetActiveUserTokensAsync(long userId, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false);

    /// <summary>
    /// Gets a token by its ID.
    /// </summary>
    /// <param name="id">The ID of the token to get.</param>
    /// <param name="includeSession">Whether to include the session in the query.</param>
    /// <param name="includeUser">Whether to include the user in the query.</param>
    /// <param name="includeAccount">Whether to include the account in the query.</param>
    /// <param name="includeApplication">Whether to include the application in the query.</param>
    /// <param name="includeApplicationAccount">Whether to include the application account in the query.</param>
    /// <param name="includeParentToken">Whether to include parent token in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the token if found, otherwise null.</returns>
    public Task<Token?> GetTokenByIdAsync(long id, bool includeSession = false, bool includeUser = false, bool includeAccount = false, bool includeApplication = false, bool includeApplicationAccount = false, bool includeParentToken = false);

    /// <summary>
    /// Gets the user associated with a session.
    /// </summary>
    /// <param name="sessionId">The ID of the session to get the user for.</param>
    /// <param name="includeAccounts">Whether to include accounts in the query.</param>
    /// <param name="includeSessions">Whether to include sessions in the query.</param>
    /// <param name="includePhoneNumbers">Whether to include phone numbers in the query.</param>
    /// <param name="includeEmailAddresses">Whether to include email addresses in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserBySessionIdAsync(long sessionId, bool includeAccounts = false, bool includeSessions = false, bool includePhoneNumbers = false, bool includeEmailAddresses = false);

    /// <summary>
    /// Gets the user associated with a token.
    /// </summary>
    /// <param name="tokenId">The ID of the token to get the user for.</param>
    /// <param name="includeAccounts">Whether to include accounts in the query.</param>
    /// <param name="includeSessions">Whether to include sessions in the query.</param>
    /// <param name="includePhoneNumbers">Whether to include phone numbers in the query.</param>
    /// <param name="includeEmailAddresses">Whether to include email addresses in the query.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserByTokenIdAsync(long tokenId, bool includeAccounts = false, bool includeSessions = false, bool includePhoneNumbers = false, bool includeEmailAddresses = false);

    /// <summary>
    /// Revokes a session, making it inactive.
    /// </summary>
    /// <param name="sessionId">The ID of the session to revoke.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RevokeSessionAsync(long sessionId);

    /// <summary>
    /// Revokes all sessions for a user, making them inactive.
    /// </summary>
    /// <param name="userId">The ID of the user to revoke all sessions for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RevokeAllUserSessionsAsync(long userId);

    /// <summary>
    /// Revokes all tokens associated with a session, making them inactive.
    /// </summary>
    /// <param name="sessionId">The ID of the session to revoke all tokens for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RevokeAllSessionTokensAsync(long sessionId);

    /// <summary>
    /// Removes a token from the database.
    /// </summary>
    /// <param name="tokenId">The ID of the token to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveTokenAsync(long tokenId);

    /// <summary>
    /// Removes a session from the database.
    /// </summary>
    /// <param name="sessionId">The ID of the session to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveSessionAsync(long sessionId);
}
