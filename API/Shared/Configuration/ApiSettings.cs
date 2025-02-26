using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// API settings for the application
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Port on which the API will listen
    /// </summary>
    [Required]
    public string Port { get; set; } = "8000";
    
    /// <summary>
    /// The base URL of the frontend application
    /// </summary>
    public string? FrontendBaseUrl { get; set; } = "http://localhost:3000";
} 