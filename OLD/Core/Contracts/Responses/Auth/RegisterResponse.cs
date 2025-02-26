namespace API.Core.Contracts.Responses.Auth;

public class RegisterResponse : ResponseBase
{
    public required long VerificationSessionID { get; set; }
}