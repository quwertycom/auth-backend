namespace Auth.Models;

public class Organization
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Avatar { get; set; } = "name";

    public required List<Account> Accounts { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}