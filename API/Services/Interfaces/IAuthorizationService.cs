
namespace API.Services.Interfaces;

public interface IAuthorizationService
{
    public Task<(bool isSuccess, string status, string message, bool isValid)> ValidateTokenAsync(string token);
}
