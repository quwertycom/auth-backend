using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class PasswordHasherSettings
{
    [Required]
    [Range(10000, int.MaxValue, ErrorMessage = "Iterations must be at least 10000 for security")]
    public int Iterations { get; set; } = 10000;
    
    [Required]
    [Range(16, int.MaxValue, ErrorMessage = "Salt size must be at least 16 bytes")]
    public int SaltSize { get; set; } = 16;
    
    [Required]
    [Range(32, int.MaxValue, ErrorMessage = "Key size must be at least 32 bytes")]
    public int KeySize { get; set; } = 32;
}