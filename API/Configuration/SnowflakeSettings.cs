using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class SnowflakeSettings
{
    [Required]
    [Range(0, 31, ErrorMessage = "DatacenterId must be between 0 and 31")]
    public long DatacenterId { get; set; } = 1;
    
    [Required]
    [Range(0, 31, ErrorMessage = "WorkerId must be between 0 and 31")]
    public long WorkerId { get; set; } = 1;
    
    [Required]
    public string Epoch { get; set; } = "2024-01-01T00:00:00Z";
}