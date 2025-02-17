using System.Security.Cryptography;

namespace API.Common.Helpers;

public static class RandomGenerator
{
    public static string GenerateNumberCode(int length = 8)
    {
        var otp = new char[length];
        byte[] randomNumber = new byte[1];

        for (int i = 0; i < length; i++)
        {
            uint num;
            do {
                RandomNumberGenerator.Fill(randomNumber);
                num = randomNumber[0];
            } while (num >= 250);

            otp[i] = (char)('0' + (num % 10));
        }
        return new string(otp);
    }

    public static string GenerateAlphanumericCode(int length = 8, bool includeSymbols = false)
    {
        const string baseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const string symbols = "!@#$%^&*()_+-=[]{}|;':\",./<>?~`";
        string chars = includeSymbols ? baseChars + symbols : baseChars;

        var result = new char[length];
        byte[] randomBuffer = new byte[length * 2];

        RandomNumberGenerator.Fill(randomBuffer);

        for (int i = 0; i < length; i++)
        {
            uint num = BitConverter.ToUInt16(randomBuffer, i * 2);
            while (num >= chars.Length)
                num >>= 1;

            result[i] = chars[(int)num];
        }
        return new string(result);
    }

    public static string GenerateSalt(int saltSize = 32)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        return Convert.ToBase64String(salt);
    }
}