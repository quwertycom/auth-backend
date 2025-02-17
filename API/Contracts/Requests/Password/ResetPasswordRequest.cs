namespace API.Contracts.Requests.Password;

public class ResetPasswordRequest
{
    public required string Code { get; set; }
    public required string NewPassword { get; set; }
}