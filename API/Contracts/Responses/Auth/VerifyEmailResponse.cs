
namespace API.Contracts.Responses.Auth;

public class VerifyEmailResponse : ResponseBase
{
    public required string Email { get; set; }
}