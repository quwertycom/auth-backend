using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Password;

public class RequestResetRequest
{
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Email must be between 3 and 50 characters")]
    public string? Email { get; set; }

    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string? Username { get; set; }
}
