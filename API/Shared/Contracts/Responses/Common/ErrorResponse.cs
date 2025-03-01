namespace API.Shared.Contracts.Responses.Common;

/// <summary>
/// Represents an error response.
/// </summary>
public record ErrorResponse : ResponseBase
{
    /// <summary>
    /// Additional details about the error.
    /// </summary>
    public IDictionary<string, string>? Details { get; set; }

    /// <summary>
    /// Timestamp of the error.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}