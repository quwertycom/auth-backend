using API.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Core.Contracts.Requests.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; set; }

    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "BirthDate is required")]
    public required DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public required UserGender Gender { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}