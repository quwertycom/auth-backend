using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Password;

public class ValidateResetCodeRequest
{
    [Required(ErrorMessage = "Code is required")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "Code must be between 3 and 128 characters")]
    public required string Code { get; set; }
}