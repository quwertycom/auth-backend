using API.Contracts.Responses;

namespace API.Contracts.Responses.Token;

public class ValidateTokenResponse : ResponseBase
{
    public bool IsValid { get; set; }
}