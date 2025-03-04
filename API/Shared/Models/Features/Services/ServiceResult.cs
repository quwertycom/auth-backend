
namespace API.Shared.Models.Features.Services;

public record ServiceResult
{
    public required bool IsSuccess { get; init; }
    public required string Status { get; init; }
    public string? Message { get; init; }
    public int? HttpStatusCode { get; init; }
    public Dictionary<string, object>? Errors { get; init; }
}