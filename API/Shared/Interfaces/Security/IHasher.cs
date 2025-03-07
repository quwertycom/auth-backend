using API.Shared.Models.Infrastructure.Hasher;

namespace API.Shared.Interfaces.Security;

/// <summary>
/// Interface for hashing passwords.
/// </summary>
public interface IHasher
{
    /// <summary>
    /// Hashes the password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="customSalt">Optional custom salt.</param>
    /// <returns>The hash and salt.</returns>
    HashResult Hash(string password, string? customSalt = null);

    /// <summary>
    /// Compares the password with the stored hash and salt.
    /// </summary>
    /// <param name="password">The password to compare.</param>
    /// <param name="storedHash">The stored hash.</param>
    /// <param name="storedSalt">The stored salt.</param>
    /// <returns>True if the password matches the hash, false otherwise.</returns>
    CompareResult Compare(string password, string storedHash, string storedSalt);
}
