using API.Core.Contracts.Responses.Common;

namespace API.Core.Contracts.Responses.Password;

public class ValidateResetCodeResponse : ResponseBase
{
    public required bool IsValid { get; set; }
}