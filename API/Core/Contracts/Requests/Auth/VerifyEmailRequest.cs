using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Auth;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "VerificationSessionID is required")]
    public required long VerificationSessionID { get; set; }

    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "OTP is required")]
    public required string OTP { get; set; }
}