using API.Shared.Models.Features.Services;

namespace API.Shared.Models.Infrastructure.Hasher;

public record CompareResult : ServiceResult
{
    public required bool IsMatch { get; set; }
}