namespace Auth.Models;

public class Developer
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Bio { get; set; }
    public required string Avatar { get; set; } = "name";
    public required bool Trusted { get; set; } = false;

    public required List<Application> Applications { get; set; } = new();
    public required List<Account> Accounts { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public required string AccountId { get; set; }
}