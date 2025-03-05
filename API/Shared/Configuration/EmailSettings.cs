namespace API.Shared.Configuration;

/// <summary>
/// Configuration settings for email services
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Default From email address for outgoing emails
    /// </summary>
    public string DefaultFromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Default From name for outgoing emails
    /// </summary>
    public string DefaultFromName { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server host
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// Whether to use SSL for SMTP connection
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// SMTP username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// SMTP connection timeout in milliseconds
    /// </summary>
    public int Timeout { get; set; } = 10000;

    /// <summary>
    /// Whether to use default credentials for SMTP authentication
    /// </summary>
    public bool UseDefaultCredentials { get; set; } = false;
}