
namespace API.Contracts.Requests.Auth;

public class VerifyEmailRequest
{
    public required long VerificationSessionID { get; set; }
    public required string Email { get; set; }
    public required string OTP { get; set; }
}