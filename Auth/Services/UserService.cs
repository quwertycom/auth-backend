using Auth.Data;
using Auth.Helpers;
using Auth.Models;
using Auth.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (await _context.Users.AnyAsync(x => x.Username == model.Username))
            {
                return (false, "USERNAME_EXISTS");
            } else if (await _context.Users.AnyAsync(x => x.Email == model.Email))
            {
                return (false, "EMAIL_EXISTS");
            } else if (model.Phone != null && await _context.Users.AnyAsync(x => x.Phone == model.Phone))
            {
                return (false, "PHONE_EXISTS");
            }

            var passwordSalt = Password.GenerateSalt();
            var passwordHash = Password.HashPassword(model.Password, passwordSalt);

            var @user = new User
            {
                Id = Snowflake.Next(),
                Username = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Avatar = "name",
                BirthDate = model.BirthDate,
                Gender = model.Gender,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Accounts = new List<Account>(),
                Sessions = new List<UserSession>(),
                Devices = new List<Device>(),
                CreatedAt = DateTime.UtcNow,
            };

            var @personalAccount = new Account
            {
                Id = Snowflake.Next(),
                Name = "Personal",
                Avatar = "name",
                User = @user,
                Sessions = new List<AccountSession>(),
                Applications = new List<ApplicationAccount>(),
                Developers = new List<Developer>(),
                CreatedAt = DateTime.UtcNow,
                UserId = @user.Id
            };

            @user.Accounts.Add(@personalAccount);

            _context.Users.Add(@user);
            await _context.SaveChangesAsync();

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