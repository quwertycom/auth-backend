using System.ComponentModel.DataAnnotations;

namespace API.Shared.Configuration;

/// <summary>
/// Configuration settings that are required to be set as environment variables for the application to run.
/// </summary>
public class RequiredEnvironmentSettings
{
    /// <summary>
    /// Hostname or IP address for the PostgreSQL database server.
    /// </summary>
    [Required(ErrorMessage = "POSTGRES_HOST is required")]
    public string PostgresHost { get; set; } = null!;

    /// <summary>
    /// Name of the PostgreSQL database.
    /// </summary>
    [Required(ErrorMessage = "POSTGRES_DB is required")]
    public string PostgresDb { get; set; } = null!;

    /// <summary>
    /// Username for PostgreSQL database authentication.
    /// </summary>
    [Required(ErrorMessage = "POSTGRES_USER is required")]
    public string PostgresUser { get; set; } = null!;

    /// <summary>
    /// Password for PostgreSQL database authentication.
    /// </summary>
    [Required(ErrorMessage = "POSTGRES_PASSWORD is required")]
    public string PostgresPassword { get; set; } = null!;

    /// <summary>
    /// Secret key used for JWT token signing. Must be at least 32 characters long for security.
    /// </summary>
    [Required(ErrorMessage = "JWT_SECRET is required")]
    [MinLength(32, ErrorMessage = "JWT_SECRET must be at least 32 characters long")]
    public string JwtSecret { get; set; } = null!;
}