namespace API.Contracts.Requests.Password;

public class ResetPasswordRequest
{
    public required long UserId { get; set; }
    public required string NewPassword { get; set; }
    public required string OTP { get; set; }
}