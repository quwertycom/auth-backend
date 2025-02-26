using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class ApiSettings
{
    /// <summary>
    /// The port on which the API will listen
    /// </summary>
    [Required]
    public string Port { get; set; } = "8000";
    
    /// <summary>
    /// The base URL of the frontend application
    /// </summary>
    public string? FrontendBaseUrl { get; set; } = "http://localhost:3000";
} 