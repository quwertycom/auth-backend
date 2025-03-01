
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.User;

namespace API.Shared.Interfaces.Database.Repositories;

/// <summary>
/// Represents a repository for managing sessions and tokens in the database.
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
    /// <returns>A task that represents the asynchronous operation. Returns the session if found, otherwise null.</returns>
    public Task<Session?> GetSessionByIdAsync(long id);

    /// <summary>
    /// Gets a session by its token string.
    /// </summary>
    /// <param name="tokenString">The token string of the session to get.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the session if found, otherwise null.</returns>
    public Task<Session?> GetSessionByTokenStringAsync(string tokenString);

    /// <summary>
    /// Gets a session by its user ID.
    /// </summary>
    /// <param name="userId">The ID of the user to get sessions for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the session if found, otherwise null.</returns>
    public Task<Session?> GetSessionByUserIdAsync(long userId);

    /// <summary>
    /// Gets all sessions for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get sessions for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of sessions.</returns>
    public Task<IEnumerable<Session>> GetAllUserSessionsAsync(long userId);

    /// <summary>
    /// Gets all active sessions for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get active sessions for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of active sessions.</returns>
    public Task<IEnumerable<Session>> GetActiveUserSessionsAsync(long userId);

    /// <summary>
    /// Gets a token by its token string.
    /// </summary>
    /// <param name="tokenString">The token string of the token to get.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the token if found, otherwise null.</returns>
    public Task<Token?> GetTokenByTokenStringAsync(string tokenString);

    /// <summary>
    /// Gets all tokens for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get tokens for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of tokens.</returns>
    public Task<IEnumerable<Token>> GetAllUserTokensAsync(long userId);

    /// <summary>
    /// Gets all active tokens for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get active tokens for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a collection of active tokens.</returns>
    public Task<IEnumerable<Token>> GetActiveUserTokensAsync(long userId);

    /// <summary>
    /// Gets a token by its ID.
    /// </summary>
    /// <param name="id">The ID of the token to get.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the token if found, otherwise null.</returns>
    public Task<Token?> GetTokenByIdAsync(long id);

    /// <summary>
    /// Gets the user associated with a session.
    /// </summary>
    /// <param name="sessionId">The ID of the session to get the user for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserBySessionIdAsync(long sessionId);

    /// <summary>
    /// Gets the user associated with a token.
    /// </summary>
    /// <param name="tokenId">The ID of the token to get the user for.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the user if found, otherwise null.</returns>
    public Task<User?> GetUserByTokenIdAsync(long tokenId);

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
