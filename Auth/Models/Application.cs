using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Models;

public class Application
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; } = "name";
    public required bool Trusted { get; set; } = false;
    public required string Status { get; set; } = "created";
    public required string Url { get; set; }

    public required Developer Developer { get; set; }

public required List<ApplicationAccount> Accounts { get; set; } = new();
    public required List<ApplicationSession> Sessions { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

[ForeignKey(nameof(Models.Developer))]
public required string DeveloperId { get; set; }
}