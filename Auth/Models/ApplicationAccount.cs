namespace Auth.Models;

public class ApplicationAccount
{
    public required string Id { get; set; }
    public required string FolderId { get; set; }

    public required Application Application { get; set; }
    public required Account Account { get; set; }

    public required List<ApplicationSession> Sessions { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public required string AccountId { get; set; }
    public required string ApplicationId { get; set; }
}