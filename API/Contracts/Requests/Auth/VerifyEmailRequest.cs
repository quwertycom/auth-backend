using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Auth;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "VerificationSessionID is required")]
    [Range(1, long.MaxValue, ErrorMessage = "VerificationSessionID must be greater than 0")]
    public required long VerificationSessionID { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 characters")]
    public required string OTP { get; set; }
}