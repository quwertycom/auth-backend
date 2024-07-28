namespace Auth.Models.ResponseModels;

public class UserLoginResponseModel
{
    public required string Status { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}