namespace Auth.Models.RequestModels;

public class UserRegisterRequestModel
{
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string BirthDate { get; set; }
    public required string Gender { get; set; }
    public required string Password { get; set; }
}