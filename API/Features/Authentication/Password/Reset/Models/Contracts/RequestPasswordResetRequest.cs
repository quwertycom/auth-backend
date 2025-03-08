
namespace API.Features.Authentication.Password.Reset.Models.Contracts;

public record RequestPasswordResetRequest
{
    public required string Email { get; set; }
}
