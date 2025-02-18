
namespace API.Contracts.Requests.Password;

public class ValidateResetCodeRequest
{
    public required string Code { get; set; }
}