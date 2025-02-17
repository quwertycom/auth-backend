namespace API.Contracts.Requests.Password;

public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
    public required string OTP { get; set; }
}