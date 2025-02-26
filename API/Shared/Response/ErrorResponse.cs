namespace API.Shared.Response;

/// <summary>
/// Represents an error response.
/// </summary>
public class ErrorResponse : ResponseBase
{
    /// <summary>
    /// Additional details about the error.
    /// </summary>
    public IDictionary<string, string>? Details { get; set; }
}