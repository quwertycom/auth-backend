namespace API.Configuration;

public class SnowflakeSettings
{
    public long DatacenterId { get; set; } = 1;
    public long WorkerId { get; set; } = 1;
    public string Epoch { get; set; } = "2024-01-01T00:00:00Z";
}