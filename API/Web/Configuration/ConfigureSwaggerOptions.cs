// This file is deprecated and is no longer used.
// Swagger configuration has been moved to API/Middleware/SwaggerExtensions.cs
// This file will be removed in a future update.

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Web.Configuration;

public class ConfigureSwaggerOptions_DEPRECATED : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IWebHostEnvironment _environment;

    public ConfigureSwaggerOptions_DEPRECATED(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // Configuration moved to SwaggerExtensions.AddSwaggerServices
    }
} 