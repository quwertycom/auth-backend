using System.ComponentModel.DataAnnotations;

namespace API.Features.Authentication.Register.Models;

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public required string Username { get; set; }
    
    [Required]
    [MinLength(8)]
    public required string Password { get; set; }
}
