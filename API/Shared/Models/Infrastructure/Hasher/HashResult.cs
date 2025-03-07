
using API.Shared.Models.Features.Services;

namespace API.Shared.Models.Infrastructure.Hasher;

public record HashResult : ServiceResult
{
    public required string Hash { get; set; }
    public required string Salt { get; set; }
}