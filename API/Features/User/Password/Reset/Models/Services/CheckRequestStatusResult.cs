using API.Shared.Models.Features.Services;

namespace API.Features.User.Password.Reset.Models.Services;

public record CheckRequestStatusResult : ServiceResult
{
    public bool? IsExpired { get; set; }
    public bool? IsUsed { get; set; }
}