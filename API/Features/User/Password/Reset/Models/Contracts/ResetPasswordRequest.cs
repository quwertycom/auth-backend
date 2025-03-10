
namespace API.Features.User.Password.Reset.Models.Contracts;

public record ResetPasswordRequest
{
    public required string Code { get; set; }
    public required string NewPassword { get; set; }
}