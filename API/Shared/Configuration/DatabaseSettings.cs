using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// Database settings for the application
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Host of the database server
    /// </summary>
    [Required]
    public string Host { get; set; } = null!;
    
    /// <summary>
    /// Name of the database
    /// </summary>
    [Required]
    public string Database { get; set; } = null!;
    
    /// <summary>
    /// Username for the database
    /// </summary>
    [Required]
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// Password for the database
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}