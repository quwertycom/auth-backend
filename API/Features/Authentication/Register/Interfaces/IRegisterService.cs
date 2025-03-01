
using API.Features.Authentication.Register.Models.Services.RegisterService;

namespace API.Features.Authentication.Register.Interfaces;

public interface IRegisterService
{
    Task<RegisterResponse> RegisterUserAsync(string username, string password, CancellationToken ct);
}