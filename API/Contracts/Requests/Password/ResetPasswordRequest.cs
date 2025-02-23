using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Password;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Code is required")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "Code must be between 3 and 128 characters")]
    public required string Code { get; set; }

    [Required(ErrorMessage = "New Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public required string NewPassword { get; set; }
}