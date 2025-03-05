
namespace API.Features.Authentication.EmailVerification.Models.Contracts;

public record RequestNewCodeRequest
{
    public required string Email { get; set; }
}
