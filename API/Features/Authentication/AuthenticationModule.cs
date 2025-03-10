using API.Features.Authentication.EmailVerification.Interfaces;
using API.Features.Authentication.EmailVerification.Services;
using API.Features.Authentication.Login.Interfaces;
using API.Features.Authentication.Login.Services;
using API.Features.Authentication.Register.Interfaces;
using API.Features.Authentication.Register.Services;
using API.Features.User.Password.Reset.Interfaces;
using API.Features.User.Password.Reset.Services;

namespace API.Features.Authentication;

/// <summary>
/// Authentication module for the API
/// </summary>
public static class AuthenticationModule
{
    /// <summary>
    /// Add all authentication services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add the services to</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();

        return services;
    }
}