using API.Features.Session.Refresh.Models.Services;

namespace API.Features.Session.Refresh.Interfaces;

public interface IRefreshSessionService
{
    Task<RefreshSessionResult> RefreshSessionAsync(string refreshToken);
}
