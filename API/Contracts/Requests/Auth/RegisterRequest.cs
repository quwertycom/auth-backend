using API.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "FirstName must be between 2 and 50 characters")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "LastName must be between 2 and 50 characters")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "BirthDate is required")]
    public required DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public required UserGender Gender { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public required string Password { get; set; }
}