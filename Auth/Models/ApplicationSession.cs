using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Models;

public class ApplicationSession
{
    public required string Id { get; set; }
    public required string RefreshToken { get; set; }
    public required string AccessToken { get; set; }
    public required string DeviceFingerprint { get; set; }
    public required bool IsRevoked { get; set; }

    public required ApplicationAccount Account { get; set; }
public required Application Application { get; set; }
public required Device Device
{ get; set; }

public required DateTime CreatedAt
{ get; set; } = DateTime.UtcNow;

[ForeignKey(nameof(Models.ApplicationAccount))]
public required string AccountId { get; set; }

[ForeignKey(nameof(Models.Application))]
public required string ApplicationId { get; set; }

[ForeignKey(nameof(Models.Device))]
public required string DeviceId { get; set; }
}