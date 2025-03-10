
using API.Shared.Contracts.Responses.Common;

namespace API.Features.User.Password.Reset.Models.Contracts;

public record CheckRequestStatusResponse : ResponseBase
{
  public bool? IsExpired { get; set; }
  public bool? IsUsed { get; set; }
}