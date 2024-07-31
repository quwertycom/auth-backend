using Auth.Data;
using Auth.Helpers;
using Auth.Models;
using Auth.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Device = Auth.Models.Device;

namespace Auth.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(bool isSuccess, string status, string? accessToken, string? refreshToken)> Login(UserLoginRequestModel model, HttpContext httpContext)
    {
        try
        {
            var @user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (@user == null)
            {
                return (false, "USER_NOT_FOUND", null, null);
            }

            var passwordHash = Password.HashPassword(model.Password, @user.PasswordSalt);
            if (@user.PasswordHash != passwordHash)
            {
                return (false, "INVALID_PASSWORD", null, null);
            }

            var screenResolution = httpContext.Request.Headers["Screen-Resolution"].ToString();
            var language = httpContext.Request.Headers["Accept-Language"].ToString();
            var platform = httpContext.Request.Headers["User-Agent"].ToString(); // Simplified for example
            var doNotTrackStatus = httpContext.Request.Headers["DNT"].ToString();

            var deviceFingerprint = Fingerprint.GenerateDeviceFingerprint(screenResolution, language, platform, doNotTrackStatus);

            var @device = await _context.Devices.FirstOrDefaultAsync(d => d.Fingerprint.Contains(deviceFingerprint) && d.User == @user);

            if (@device != null)
            {
                @device.LastUsed = DateTime.UtcNow;

                var newSessionId = Snowflake.Next();

                var refreshToken = JWT.User.GenerateRefreshToken(@user.Id, newSessionId, _configuration["JWT:Audience"]??"unknown");
                var accessToken = JWT.User.GenerateAccessToken(@user.Id, newSessionId, _configuration["JWT:Audience"]??"unknown");

                var @newSession = new UserSession
                {
                    Id = newSessionId,
                    RefreshToken = refreshToken,
                    AccessToken = accessToken,
                    DeviceFingerprint = deviceFingerprint,
                    IsRevoked = false,
                    User = @user,
                    Device = @device,
                    CreatedAt = DateTime.UtcNow,
                    UserId = @user.Id,
                    DeviceId = @device.Id,
                };

                @device.UserSessions.Add(@newSession);
                @user.Sessions.Add(@newSession);

                _context.UserSessions.Add(@newSession);

                await _context.SaveChangesAsync();

                return (true, "OK", accessToken, refreshToken);
            }
            else
            {
                string deviceName = "";

                var deviceMappings = new Dictionary<string, string>
                {
                    { "Windows", "Windows" },
                    { "Mac", "Mac" },
                    { "Linux", "Linux" },
                    { "Android", "Android Phone" },
                    { "iOS", "iPhone" }
                };

                foreach (var mapping in deviceMappings)
                {
                    if (platform.Contains(mapping.Key))
                    {
                        deviceName = mapping.Value;
                        break;
                    }
                }

                var @newDevice = new Device
                {
                    Id = Snowflake.Next(),
                    Name = deviceName,
                    LastUsed = DateTime.UtcNow,
                    Platform = platform,
                    User = @user,
                    Fingerprint = new List<string>
                    {
                        deviceFingerprint
                    },
                    AccountSessions = new List<AccountSession>(),
                    UserSessions = new List<UserSession>(),
                    ApplicationSessions = new List<ApplicationSession>(),
                    CreatedAt = DateTime.UtcNow,
                    UserId = @user.Id
                };

                var newSessionId = Snowflake.Next();

                var refreshToken = JWT.User.GenerateRefreshToken(@user.Id, newSessionId, _configuration["JWT:Audience"]??"unknown");
                var accessToken = JWT.User.GenerateAccessToken(@user.Id, newSessionId, _configuration["JWT:Audience"]??"unknown");

                var @newSession = new UserSession
                {
                    Id = newSessionId,
                    RefreshToken = refreshToken,
                    AccessToken = accessToken,
                    DeviceFingerprint = deviceFingerprint,
                    IsRevoked = false,
                    User = @user,
                    Device = @newDevice,
                    CreatedAt = DateTime.UtcNow,
                    UserId = @user.Id,
                    DeviceId = @newDevice.Id,
                };

                @newDevice.UserSessions.Add(@newSession);
                @user.Sessions.Add(@newSession);

                _context.UserSessions.Add(@newSession);
                _context.Devices.Add(@newDevice);

                await _context.SaveChangesAsync();

                return (true, "OK", accessToken, refreshToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
    Task<(bool isSuccess, string status, string? accessToken, string? refreshToken)> Login(UserLoginRequestModel model, HttpContext httpContext);
    Task<(bool isSuccess, string status)> Register(UserRegisterRequestModel model);

}