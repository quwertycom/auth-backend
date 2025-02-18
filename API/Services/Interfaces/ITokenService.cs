
namespace API.Services.Interfaces;

public interface ITokenService
{
    public Task<(bool isSuccess, string status, string message, bool isValid)> ValidateAsync(string token);
}
