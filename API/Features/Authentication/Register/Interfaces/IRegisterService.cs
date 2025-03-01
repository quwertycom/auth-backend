
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.Register.Models.Services;

namespace API.Features.Authentication.Register.Interfaces;

public interface IRegisterService
{
    Task<Models.Services.RegisterResponse> RegisterUserAsync(RegisterRequest request, CancellationToken ct);
}