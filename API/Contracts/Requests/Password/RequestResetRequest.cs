
namespace API.Contracts.Requests.Password;

public class RequestResetRequest
{
    public string? Email { get; set; }
    public string? Username { get; set; }
}
