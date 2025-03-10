
namespace API.Features.User.Password.Reset.Models.Contracts;

public record RequestPasswordResetRequest
{
    public string? Email { get; set; }
    public string? Username { get; set; }
}
