namespace API.Shared.Contracts.Responses.Common;

/// <summary>
/// Represents a base response.
/// </summary>
public record ResponseBase
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Message of the response with more details. Include only if needed.
    /// </summary>
    public string? Message { get; set; }
}