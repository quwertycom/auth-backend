namespace Auth.Models.RequestModels;

public class UserLoginRequestModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}