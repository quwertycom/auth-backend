using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class RequiredEnvironmentSettings
{
    [Required(ErrorMessage = "POSTGRES_HOST is required")]
    public string PostgresHost { get; set; } = null!;

    [Required(ErrorMessage = "POSTGRES_DB is required")]
    public string PostgresDb { get; set; } = null!;

    [Required(ErrorMessage = "POSTGRES_USER is required")]
    public string PostgresUser { get; set; } = null!;

    [Required(ErrorMessage = "POSTGRES_PASSWORD is required")]
    public string PostgresPassword { get; set; } = null!;

    [Required(ErrorMessage = "JWT_SECRET is required")]
    [MinLength(32, ErrorMessage = "JWT_SECRET must be at least 32 characters long")]
    public string JwtSecret { get; set; } = null!;
} 