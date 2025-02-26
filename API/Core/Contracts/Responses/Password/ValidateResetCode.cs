using API.Contracts.Responses.Common;

namespace API.Contracts.Responses.Password;

public class ValidateResetCodeResponse : ResponseBase
{
    public required bool IsValid { get; set; }
}