using API.Shared.Enums.Entities.User;

namespace API.Features.Authentication.Register.Models;

public record RegisterRequest
{
    public required string Username { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public required DateTime BirthDate { get; init; }
    public required UserGender Gender { get; init; }
    public required string Password { get; init; }
}
