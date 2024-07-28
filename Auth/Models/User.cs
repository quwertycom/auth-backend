namespace Auth.Models;

public class User
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Avatar { get; set; } = "name";
    public required string BirthDate { get; set; }
    public required string Gender { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }

    public required List<Account> Accounts { get; set; } = new();
    public required List<UserSession> Sessions { get; set; } = new();
    public required List<Device> Devices { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}