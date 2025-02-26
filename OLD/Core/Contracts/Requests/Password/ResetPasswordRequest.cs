using System.ComponentModel.DataAnnotations;

namespace API.Core.Contracts.Requests.Password;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Code is required")]
    public required string Code { get; set; }

    [Required(ErrorMessage = "New Password is required")]
    public required string NewPassword { get; set; }
}