namespace API.Shared.Response;

/// <summary>
/// Represents a base response.
/// </summary>
public class ResponseBase
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Message of the response.
    /// </summary>
    public required string Message { get; set; }
}