namespace API.Shared.Configuration;

/// <summary>
/// Email settings for the application
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Host of the email server
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Port of the email server
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Enable SSL for email communication
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// The username for the email server
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password for the email server
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Email address used as the sender
    /// </summary>
    public string FromEmail { get; set; } = null!;

    /// <summary>
    /// Timeout for email operations in milliseconds (default: 30000)
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Use default credentials for email server authentication (default: false)
    /// </summary>
    public bool UseDefaultCredentials { get; set; } = false;
}