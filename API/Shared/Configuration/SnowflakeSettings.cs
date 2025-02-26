using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// Settings for Snowflake ID generator.
/// </summary>
public class SnowflakeSettings
{
    /// <summary>
    /// Datacenter ID for the Snowflake ID generator. Must be between 0 and 31.
    /// </summary>
    [Required]
    [Range(0, 31, ErrorMessage = "DatacenterId must be between 0 and 31")]
    public long DatacenterId { get; set; } = 1;

    /// <summary>
    /// Worker ID for the Snowflake ID generator. Must be between 0 and 31.
    /// </summary>
    [Required]
    [Range(0, 31, ErrorMessage = "WorkerId must be between 0 and 31")]
    public long WorkerId { get; set; } = 1;

    /// <summary>
    /// Epoch for the Snowflake ID generator.  This is the starting timestamp for ID generation.
    /// </summary>
    [Required]
    public string Epoch { get; set; } = "2024-01-01T00:00:00Z";
}