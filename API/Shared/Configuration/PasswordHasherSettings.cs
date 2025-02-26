using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// Settings for password hashing
/// </summary>
public class PasswordHasherSettings
{
    /// <summary>
    /// Number of iterations used in the password hashing algorithm. Higher values increase security but also computation time.
    /// </summary>
    [Required]
    [Range(10000, int.MaxValue, ErrorMessage = "Iterations must be at least 10000 for security")]
    public int Iterations { get; set; } = 10000;
    
    /// <summary>
    /// Size of the salt in bytes used in the password hashing algorithm.
    /// </summary>
    [Required]
    [Range(16, int.MaxValue, ErrorMessage = "Salt size must be at least 16 bytes")]
    public int SaltSize { get; set; } = 16;
    
    /// <summary>
    /// Size of the key in bytes derived from the password and salt during hashing.
    /// </summary>
    [Required]
    [Range(32, int.MaxValue, ErrorMessage = "Key size must be at least 32 bytes")]
    public int KeySize { get; set; } = 32;
}