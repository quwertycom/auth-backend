using API.Core.Enums;
namespace API.Core.Services.Interfaces;

public interface ITokenService
{
    public Task<(bool isSuccess, string status, string message, bool isValid, TokenType? tokenType, TokenTarget? tokenTarget)> ValidateAsync(string token);
}
