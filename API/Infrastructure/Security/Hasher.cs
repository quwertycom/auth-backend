using System.Security.Cryptography;
using API.Shared.Configuration;
using API.Shared.Interfaces.Security;
using Microsoft.Extensions.Options;


namespace API.Infrastructure.Security;

public class Hasher : IHasher
{
    private bool _isInitialized;
    private int _iterations;
    private int _saltSize;
    private int _keySize;
    private IRandomGenerator _randomGenerator;

    public Hasher(IRandomGenerator randomGenerator)
    {
        _randomGenerator = randomGenerator;
    }

    public void Initialize(IConfiguration configuration)
    {
        if (_isInitialized) return;

        var settings = configuration.GetSection("PasswordHasher").Get<PasswordHasherSettings>()
            ?? throw new InvalidOperationException("PasswordHasher settings are not configured");

        InitializeWithSettings(settings);
    }

    public void Initialize(IOptions<PasswordHasherSettings> options)
    {
        if (_isInitialized) return;
        
        var settings = options.Value;
        InitializeWithSettings(settings);
    }
    
    private void InitializeWithSettings(PasswordHasherSettings settings)
    {
        _iterations = settings.Iterations;
        _saltSize = settings.SaltSize;
        _keySize = settings.KeySize;
        
        // Validation is now handled by DataAnnotations in the PasswordHasherSettings class
        _isInitialized = true;
    }

    public (string hash, string salt) Hash(string password, string? customSalt = null)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("PasswordHasher is not initialized. Call Initialize() first.");
        }

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
