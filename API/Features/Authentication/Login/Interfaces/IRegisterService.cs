using API.Features.Authentication.Login.Models.Contracts;
using API.Features.Authentication.Login.Models.Services;

namespace API.Features.Authentication.Login.Interfaces;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken);
}
