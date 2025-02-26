using API.Core.Contracts.Responses;

namespace API.Core.Contracts.Responses.Token;

public class ValidateTokenResponse : ResponseBase
{
    public bool IsValid { get; set; }
}