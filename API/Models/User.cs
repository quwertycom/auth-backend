public class User {
    public required long Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}