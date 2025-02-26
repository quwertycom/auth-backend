namespace API.Core.Contracts.Responses;

public class ResponseBase
{
    public required string Status { get; set; }
    public required string Message { get; set; }
}