
namespace API.Features.Authentication.Register.Interfaces;

public interface IRegisterService
{
    Task<string> RegisterUserAsync(string username, string password, CancellationToken ct);
}