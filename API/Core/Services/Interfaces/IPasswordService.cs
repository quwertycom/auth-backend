namespace API.Core.Services.Interfaces;

public interface IPasswordService
{
    public Task<(bool isSuccess, string status, string message)> RequestResetViaEmail(string Email);
    public Task<(bool isSuccess, string status, string message)> RequestResetViaUsername(string Username);
    public Task<(bool isSuccess, string status, string message, bool isValid)> ValidateResetCode(string code);
    public Task<(bool isSuccess, string status, string message)> ChangePassword(string code, string Password);
}