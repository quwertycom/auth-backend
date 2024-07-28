using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Models;

public class ApplicationAccount
{
    public required string Id { get; set; }
    public required string FolderId { get; set; }

    public required Application Application { get; set; }
    public required Account Account { get; set; }

    public required List<ApplicationSession> Sessions { get; set; } = new();

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Models.Account))]
    public required string AccountId { get; set; }

    [ForeignKey(nameof(Models.Application))]
    public required string ApplicationId { get; set; }
}