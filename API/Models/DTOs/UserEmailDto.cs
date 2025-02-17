using API.Common.Enums;

public class UserEmailDto
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public EmailType Type { get; set; }
    public EmailState State { get; set; }
    public long UserId { get; set; }
}