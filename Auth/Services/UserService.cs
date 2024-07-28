using Auth.Data;
using Auth.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool isSuccess, string status, string? accessToken, string? refreshToken)> Login(UserLoginRequestModel model)
    {
        try
        {
            return (true, "OK", "accessToken", "refreshToken");
        }
        catch
        {
            return (false, "ERROR", null, null);
        }
    }

    public async Task<(bool isSuccess, string status)> Register(UserRegisterRequestModel model)
    {
        try
        {
            return (true, "OK");
        }
        catch
        {
            return (false, "ERROR");
        }
    }
}

public interface IUserService
{
    Task<(bool isSuccess, string status, string? accessToken, string? refreshToken)> Login(UserLoginRequestModel model);
    Task<(bool isSuccess, string status)> Register(UserRegisterRequestModel model);

}