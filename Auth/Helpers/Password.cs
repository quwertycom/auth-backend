using System;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Helpers;

public static class Password
{
    public static string GenerateSalt(int size = 32)
    {
        var rng = new RNGCryptoServiceProvider();
        var saltBytes = new byte[size];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = $"{password}{salt}";
            var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hashBytes = sha256.ComputeHash(saltedPasswordBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var hashOfInput = HashPassword(password, storedSalt);
        return hashOfInput == storedHash;
    }
}