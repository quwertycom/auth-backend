namespace API.Services.Interfaces;

public interface IPasswordService
{
    public Task<(bool isSuccess, string status, string message)> ChangePassword(long UsertId, string Password, string otp);
    public Task<(bool isSuccess, string status, string message)> RequestResetViaEmail(string Email);
    public Task<(bool isSuccess, string status, string message)> RequestResetViaUsername(string Username);
} 