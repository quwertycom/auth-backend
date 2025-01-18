using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace API.Common.Helpers;

public static class PasswordHasher
{
    private static bool _isInitialized;
    private static int _iterations;
    private static int _saltSize;
    private static int _keySize;

    public static void Initialize(IConfiguration configuration)
    {
        if (_isInitialized) return;

        // Load configuration values directly
        _iterations = int.Parse(configuration["ENV__PASSWORD_HASHER__ITERATIONS"] ?? "10000");
        _saltSize = int.Parse(configuration["ENV__PASSWORD_HASHER__SALT_SIZE"] ?? "16");
        _keySize = int.Parse(configuration["ENV__PASSWORD_HASHER__KEY_SIZE"] ?? "32");

        // Validate parameters
        if (_iterations < 10000)
        {
            throw new ArgumentException("Iterations must be at least 10000 for security");
        }

        if (_saltSize < 16)
        {
            throw new ArgumentException("Salt size must be at least 16 bytes");
        }

        if (_keySize < 32)
        {
            throw new ArgumentException("Key size must be at least 32 bytes");
        }

        _isInitialized = true;
    }

    public static (string hash, string salt) Hash(string password)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("PasswordHasher is not initialized. Call Initialize() first.");
        }

        // Generate a random salt
        byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
        string saltBase64 = Convert.ToBase64String(salt);

        // Hash the password with the salt
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _iterations,
            HashAlgorithmName.SHA512,
            _keySize
        );

        return (Convert.ToBase64String(hash), saltBase64);
    }

    public static bool Compare(string password, string storedHash, string storedSalt)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("PasswordHasher is not initialized. Call Initialize() first.");
        }

        try
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            byte[] hash = Convert.FromBase64String(storedHash);

            // Hash the input password with the same salt
            byte[] newHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                _iterations,
                HashAlgorithmName.SHA512,
                _keySize
            );

            // Compare the hashes
            return CryptographicOperations.FixedTimeEquals(hash, newHash);
        }
        catch
        {
            return false;
        }
    }
}
