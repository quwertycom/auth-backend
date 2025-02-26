namespace API.Configuration;

public class EmailSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromEmail { get; set; } = null!;

    // Optional settings with default values
    public int Timeout { get; set; } = 30000; // 30 seconds
    public bool UseDefaultCredentials { get; set; } = false;
}