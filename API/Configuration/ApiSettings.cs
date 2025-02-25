using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class ApiSettings
{
    /// <summary>
    /// The port on which the API will listen
    /// </summary>
    [Required]
    public string Port { get; set; } = "8000";
} 