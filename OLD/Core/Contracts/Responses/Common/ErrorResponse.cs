namespace API.Core.Contracts.Responses.Common;

public class ErrorResponse : ResponseBase
{
    public IDictionary<string, string>? Details { get; set; }
}