using Auth.Services;

namespace Auth.Helpers;

public static class Services
{
    public static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
    }
}