
namespace API.Common.Helpers;

public static class OTPGenerator {
    public static string GenerateOTP() {
        var otp = new char[8];
        var randomNumber = new byte[1];
        for (int i = 0; i < otp.Length; i++) {
            System.Security.Cryptography.RandomNumberGenerator.Fill(randomNumber);
            otp[i] = (char)('0' + (randomNumber[0] % 10));
        }
        return new string(otp);
    }
}