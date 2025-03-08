
namespace API.Features.Authentication.Password.Reset.Models.Contracts;

public class RequestPasswordResetRequest
{
    public required string Email { get; set; }
}
