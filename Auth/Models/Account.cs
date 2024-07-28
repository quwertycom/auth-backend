using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Models;

public class Account
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Avatar { get; set; } = "name";
    public string? OrganizationRole { get; set; }

    public required User User { get; set; }
    public Organization? Organization { get; set; }

    public required List<AccountSession> Sessions { get; set; } = new();
    public required List<ApplicationAccount> Applications { get; set; } = new();
    public required List<Developer> Developers { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Models.Organization))]
    public string? OrganizationId { get; set; }

    [ForeignKey(nameof(Models.User))]
    public required string UserId { get; set; }
}