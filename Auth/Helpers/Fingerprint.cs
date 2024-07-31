using System.Security.Cryptography;
using System.Text;

namespace Auth.Helpers;

public class Fingerprint
{
    public static string GenerateDeviceFingerprint(
        string screenResolution,
        string language,
        string platform,
        string doNotTrackStatus)
    {
        var fingerprintData = new StringBuilder();
        fingerprintData.Append(screenResolution);
        fingerprintData.Append(language);
        fingerprintData.Append(platform);
        fingerprintData.Append(doNotTrackStatus);

        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprintData.ToString()));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}