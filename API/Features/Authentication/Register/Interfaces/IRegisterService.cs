
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.Register.Models.Services;

namespace API.Features.Authentication.Register.Interfaces;

public interface IRegisterService
{
    Task<RegisterResult> RegisterUserAsync(RegisterRequest request, CancellationToken ct);
}