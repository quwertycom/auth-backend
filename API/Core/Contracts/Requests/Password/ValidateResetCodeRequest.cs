using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Password;

public class ValidateResetCodeRequest
{
    [Required(ErrorMessage = "Code is required")]
    public required string Code { get; set; }
}