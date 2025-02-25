using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class RateLimitingSettings
{
    [Range(1, int.MaxValue, ErrorMessage = "PermitLimit must be greater than 0")]
    public int PermitLimit { get; set; } = 20;
    
    [Range(1, int.MaxValue, ErrorMessage = "SegmentsPerWindow must be greater than 0")]
    public int SegmentsPerWindow { get; set; } = 4;
    
    public int WindowInMinutes { get; set; } = 1;
} 