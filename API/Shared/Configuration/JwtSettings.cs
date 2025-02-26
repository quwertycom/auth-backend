using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// JWT settings for authentication and authorization
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key used to sign JWT tokens. Must be at least 32 characters long.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// Issuer of the JWT token
    /// </summary>
    [Required]
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Audience of the JWT token
    /// </summary>
    [Required]
    public string Audience { get; set; } = null!;
}