namespace API.Contracts.Responses.Common;

public class ErrorResponse
{
    public required string Status { get; set; }
    public required string Message { get; set; }
    public IDictionary<string, string>? Details { get; set; }
}