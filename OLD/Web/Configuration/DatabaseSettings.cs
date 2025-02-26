using System.ComponentModel.DataAnnotations;

namespace API.Web.Configuration;

public class DatabaseSettings
{
    [Required]
    public string Host { get; set; } = null!;
    
    [Required]
    public string Database { get; set; } = null!;
    
    [Required]
    public string Username { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
}