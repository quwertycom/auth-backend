using System.Security.Cryptography;
using API.Shared.Configuration;
using API.Shared.Interfaces.Security;
using Microsoft.Extensions.Options;


namespace API.Infrastructure.Security;

public class Hasher : IHasher
{
    private readonly int _iterations;
    private readonly int _saltSize;
    private readonly int _keySize;
    private readonly IRandomGenerator _randomGenerator;

    public Hasher(IRandomGenerator randomGenerator, IOptions<PasswordHasherSettings> options)
    {
        _randomGenerator = randomGenerator;
        
        var settings = options.Value;
        _iterations = settings.Iterations;
        _saltSize = settings.SaltSize;
        _keySize = settings.KeySize;
    }

    public (string hash, string salt) Hash(string password, string? customSalt = null)
    {
        string saltBase64;
        byte[] salt;

        if (customSalt == null)
        {
            // Generate a random salt
            saltBase64 = _randomGenerator.GenerateSalt(_saltSize);
            salt = Convert.FromBase64String(saltBase64);
        }
        else if (customSalt == "")
        {
            saltBase64 = "";
            salt = Array.Empty<byte>();
        }
        else
        {
            saltBase64 = customSalt;
            salt = Convert.FromBase64String(saltBase64);
        }

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

    public bool Compare(string password, string storedHash, string storedSalt)
    {
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
