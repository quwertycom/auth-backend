using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;

namespace API.Web.Middleware;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "qAuth API", 
                Version = "v1",
                Description = "Authentication service API"
            });
            
            // Add JWT authentication support to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        return services;
    }

    public static IApplicationBuilder UseSwaggerServices(this IApplicationBuilder app)
    {
        // Configure Swagger JSON endpoint with the custom route
        app.UseSwagger(c => {
            c.RouteTemplate = "api/docs/{documentName}/swagger.json";
        });
        
        // Configure Swagger UI
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/api/docs/v1/swagger.json", "qAuth API v1");
            c.RoutePrefix = "api/docs"; // Set UI route prefix to /api/docs
        });
        
        return app;
    }
} 