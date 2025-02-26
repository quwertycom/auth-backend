using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// Settings for rate limiting
/// </summary>
public class RateLimitingSettings
{
    /// <summary>
    /// Maximum number of requests allowed within a time window.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "PermitLimit must be greater than 0")]
    public int PermitLimit { get; set; } = 20;
    
    /// <summary>
    /// Number of segments per time window used for rate limiting.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "SegmentsPerWindow must be greater than 0")]
    public int SegmentsPerWindow { get; set; } = 4;
    
    /// <summary>
    /// Duration of the rate limiting window in minutes.
    /// </summary>
    public int WindowInMinutes { get; set; } = 1;
}