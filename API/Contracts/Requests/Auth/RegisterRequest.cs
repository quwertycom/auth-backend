using API.Common.Enums;

namespace API.Contracts.Requests.Auth;

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public required DateTime BirthDate { get; set; }
    public required UserGender Gender { get; set; }
    public required string Password { get; set; }
}